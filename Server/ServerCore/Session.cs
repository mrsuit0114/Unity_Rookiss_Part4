using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;
        // [size(2)][packet(2)]
        public sealed override int OnRecv(ArraySegment<byte> buffer)  // sealed -> 상속받은 클래스가 오버라이드 못함
        {
            int processLen = 0;
            int packetCount = 0;

            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                // 패킷이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔으면 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array,buffer.Offset,dataSize));
                packetCount++;
                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array,buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            if(packetCount> 1)
                Console.WriteLine($"packet stack send : {packetCount}");

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte>buffer);
    }



    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint) ;

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }



        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // recvArgs.UserToken -> 전달하고싶은 아무 데이터나 넣을 수 있음 - object

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();

        }

        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0)
                return;
            lock (_lock)  // 락을 가질때까지 대기하므로 ..1
            {
                foreach(ArraySegment<byte> sendBuff in sendBuffList)
                    _sendQueue.Enqueue(sendBuff);

                if (_pendingList.Count == 0)  // pending == false
                    RegisterSend();
            }
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock(_lock)  // 락을 가질때까지 대기하므로 ..1
            {
                _sendQueue.Enqueue(sendBuff);  // 이건 락 밖에 있어야 하는게 아닌가? -> 락 안에있어도된다. ..2
                if (_pendingList.Count == 0)  // pending == false
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            // 같은 세션에 대해 두번 이상 Disconnect가 불렸을 경우 조치
            if(Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }

        #region network cm

        void RegisterSend()
        {
            if (_disconnected == 1) return;

            //byte[] buff = _sendQueue.Dequeue();
            //_sendArgs.SetBuffer(buff, 0, buff.Length);  // 버퍼 하나씩 전달하는 방식

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                //_sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
                _pendingList.Add(buff);  // 좀 더 큰 단위로 전달하는 방식
                // BufferList.Add를 이용하면 이상한 방식으로 작동한다함, list를 전달하는 방식으로 하는 것을 권함
                // 왜 ArraySegment를 이용해야 하는가 -> 그냥 해당 버퍼를 시작idx와 복사하는 길이 정할때 쓰는듯
            }

            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);  // 실제 보내는 코드
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e) { Console.WriteLine($"RegisterSend Failed {e}"); }
        }

        void OnSendCompleted(object? sender, SocketAsyncEventArgs args)  // Send를 통해서오는게 아니라 이벤트핸들러를 통해 올 경우를
            // 대비해서 여기도 락을 걸어줌
        {
            lock (_lock)
            {

                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            RegisterSend();


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            if(_disconnected == 1) return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }catch (Exception e) { Console.WriteLine($"RegisterRecv Failed {e}"); };
        
        }

        void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            // 연결을 끊는 경우 받은 바이트가 0 일수 있음
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    //Write 커서 이동
                    if(_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터 넘겨주고 얼마나 처리했는지
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if(processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if(_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();

                }catch (Exception e)
                {
                    Console.WriteLine(e.ToString() );
                }
            }
            else
            {
                Disconnect();
            }
        }

    }
    #endregion
}
