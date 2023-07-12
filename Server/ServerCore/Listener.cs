using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(10);

            // 처음만 직접 시도하고 이후는 메서드끼리 서로 물면서 호출함
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);  // 이벤트를 달아서 확정가능
            RegisterAccept(args);  // 한번 시도해봄 되면 좋고 안되면 이벤트핸들러를 통해 연결됨을 확인할 것

        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;  // args를 재사용할때 밀어줘야함!! 

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)  // 바로 완료된 경우
                OnAcceptCompleted(null, args);
        }

        void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)  // 잘 처리됨
            {
                _onAcceptHandler.Invoke(args.AcceptSocket!);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            Console.WriteLine("dsadas?");
            RegisterAccept(args);
        
        }

        public Socket Accept()
        {
            return _listenSocket.Accept();
        }



    }
}
