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

connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); },10);

while (true)
{
    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try
    {
        SessionManager.Instance.SendForEach();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }

    Thread.Sleep(250);  // 일반적인 mmo에서 이동패킷을 1초에 4번 전송한다고함
}
