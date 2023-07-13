using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint) ;



        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // recvArgs.UserToken -> 전달하고싶은 아무 데이터나 넣을 수 있음 - object
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();

        }

        public void Send(byte[] sendBuff)
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
        }

        #region network cm

        void RegisterSend()
        {
            //byte[] buff = _sendQueue.Dequeue();
            //_sendArgs.SetBuffer(buff, 0, buff.Length);  // 버퍼 하나씩 전달하는 방식

            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                //_sendArgs.BufferList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
                _pendingList.Add(new ArraySegment<byte>(buff,0, buff.Length));  // 좀 더 큰 단위로 전달하는 방식
                // BufferList.Add를 이용하면 이상한 방식으로 작동한다함, list를 전달하는 방식으로 하는 것을 권함
                // 왜 ArraySegment를 이용해야 하는가 -> 그냥 해당 버퍼를 시작idx와 복사하는 길이 정할때 쓰는듯
            }

            _sendArgs.BufferList = _pendingList;


            bool pending = _socket.SendAsync(_sendArgs);  // 실제 보내는 코드
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
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
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        
        }

        void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            // 연결을 끊는 경우 받은 바이트가 0 일수 있음
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
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
