// See https://aka.ms/new-console-template for more information
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

connector.Connect(endPoint, () => { return new GameSession(); });

while (true)
{
    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try
    {
        socket.Connect(endPoint);  // 여기도 연결할때까지 대기하나봄
        Console.WriteLine($"Connected To {socket.RemoteEndPoint}");

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }
    Thread.Sleep(1000);

}

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");

        for (int i = 0; i < 5; i++)
        {
            byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World!{i} ");
            Send(sendBuff);
        }
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }

    public override void OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Server] {recvData}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Transferred bytes: {numOfBytes} OnSend");
    }
}
