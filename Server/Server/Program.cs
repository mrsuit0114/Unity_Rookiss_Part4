// See https://aka.ms/new-console-template for more information

using Server;
using Server.Session;
using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program{
    static Listener _listener = new Listener();
    public static GameRoom Room = new GameRoom();

    static void FlushRoom()
    {
        Room.Push(() => Room.Flush());
        JobTimer.Instance.Push(FlushRoom, 250);
    }

    static void Main(string[] args)
    {
        string host = Dns.GetHostName();
        //Console.WriteLine(host);
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        //Console.WriteLine(ipHost);
        IPAddress ipAddr = ipHost.AddressList[0];
        //Console.WriteLine(ipAddr);
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


        _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
        Console.WriteLine("Listening...");

        //FlushRoom();
        JobTimer.Instance.Push(FlushRoom);

        while (true)
        {
            JobTimer.Instance.Flush();
        }
    }
}

