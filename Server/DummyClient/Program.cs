// See https://aka.ms/new-console-template for more information
using DummyClient;
using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

string host = Dns.GetHostName();
//Console.WriteLine(host);
IPHostEntry ipHost = Dns.GetHostEntry(host);
//Console.WriteLine(ipHost);
IPAddress ipAddr = ipHost.AddressList[0];
//Console.WriteLine(ipAddr);
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

Connector connector = new Connector();

connector.Connect(endPoint, () => { return new ServerSession(); });

while (true)
{
    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try
    {
        socket.Connect(endPoint);  // 여기도 연결할때까지 대기하나봄
        Console.WriteLine($"Connected To {socket.RemoteEndPoint}");

        Thread.Sleep(100);

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }

}
