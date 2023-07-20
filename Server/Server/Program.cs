// See https://aka.ms/new-console-template for more information

using Server;
using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

Listener _listener = new Listener();

PacketManager.Instance.Register();

string host = Dns.GetHostName();
//Console.WriteLine(host);
IPHostEntry ipHost = Dns.GetHostEntry(host);
//Console.WriteLine(ipHost);
IPAddress ipAddr = ipHost.AddressList[0];
//Console.WriteLine(ipAddr);
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


_listener.Init(endPoint, () => { return new ClientSession(); });
Console.WriteLine("Listening...");

while (true)
{
    ;
}

