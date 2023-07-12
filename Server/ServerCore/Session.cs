using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Session
    {
        Socket _socket;
        int _disconnected = 0;

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // recvArgs.UserToken -> 전달하고싶은 아무 데이터나 넣을 수 있음 - object
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv(recvArgs);

        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
        }

        public void Disconnect()
        {
            // 같은 세션에 대해 두번 이상 Disconnect가 불렸을 경우 조치
            if(Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region network cm
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvCompleted(null, args);
        
        }

        void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            // 연결을 끊는 경우 받은 바이트가 0 일수 있음
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv(args);

                }catch (Exception e)
                {
                    Console.WriteLine(e.ToString() );
                }
            }
            else
            {

            }
        }

    }
    #endregion
}
