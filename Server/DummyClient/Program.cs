// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

string host = Dns.GetHostName();
Debug.WriteLine(host);
IPHostEntry ipHost = Dns.GetHostEntry(host);
Debug.WriteLine(ipHost);
IPAddress ipAddr = ipHost.AddressList[0];
Debug.WriteLine(ipAddr);
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

Socket socket = new Socket(endPoint.AddressFamily,SocketType.Stream, ProtocolType.Tcp);

try
{
    socket.Connect(endPoint);  // 여기도 연결할때까지 대기하나봄
    Console.WriteLine($"Connected To {socket.RemoteEndPoint}");

    byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World!");
    int sendBytes = socket.Send(sendBuff);

    byte[] recvBuff = new byte[1024];
    int recvBytes = socket.Receive(recvBuff);
    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
    Console.WriteLine($"[From Server] {recvData}");

    socket.Shutdown(SocketShutdown.Both);
    socket.Close();
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
