// See https://aka.ms/new-console-template for more information


using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Code Start") ;
string host = Dns.GetHostName();
Console.WriteLine(host);
IPHostEntry ipHost = Dns.GetHostEntry(host);
Console.WriteLine(ipHost);
IPAddress ipAddr = ipHost.AddressList[0];
Console.WriteLine(ipAddr);
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

Console.WriteLine("Code Start") ;
//문지기
Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

try
{
    // 문지기 교육
    listenSocket.Bind(endPoint);

    // 영업시작, 최대 대기수
    listenSocket.Listen(10);

    while (true)
    {
        Console.WriteLine("Listening...");

        Socket clientSocket = listenSocket.Accept();  // 클라이언트 접속이 안하면 리턴이안됨 -> 해당코드에서 대기

        byte[] recvBuff = new byte[1024];
        int recvBytes = clientSocket.Receive(recvBuff);
        string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
        Console.WriteLine($"[From Client] : {recvData}");

        byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
        clientSocket.Send(sendBuff);

        // 연결 끊기
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}

