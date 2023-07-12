// See https://aka.ms/new-console-template for more information


using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

Listener _listener = new Listener();

void OnAcceptHandler(Socket clientSocket)
{
    try
    {
        Session session = new Session();
        session.Start(clientSocket);

        byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
        session.Send(sendBuff);

        Thread.Sleep(1000);

        session.Disconnect();

    }
    catch(Exception e)
    {
        Console.WriteLine(e.ToString());
    }
}


string host = Dns.GetHostName();
Console.WriteLine(host);
IPHostEntry ipHost = Dns.GetHostEntry(host);
Console.WriteLine(ipHost);
IPAddress ipAddr = ipHost.AddressList[0];
Console.WriteLine(ipAddr);
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


_listener.Init(endPoint, OnAcceptHandler);

while (true)
{
    ;
}


