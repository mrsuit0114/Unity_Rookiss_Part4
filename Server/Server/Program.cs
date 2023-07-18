// See https://aka.ms/new-console-template for more information

using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

Listener _listener = new Listener();

string host = Dns.GetHostName();
//Console.WriteLine(host);
IPHostEntry ipHost = Dns.GetHostEntry(host);
//Console.WriteLine(ipHost);
IPAddress ipAddr = ipHost.AddressList[0];
//Console.WriteLine(ipAddr);
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);


_listener.Init(endPoint, () => { return new GameSession(); });
Console.WriteLine("Listening...");

while (true)
{
    ;
}

class Packet
{
    public ushort size;
    public ushort packetId;
}


class GameSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");

        //Packet packet = new Packet() { size = 100, packetId = 10 };

        //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
        //byte[] buffer = BitConverter.GetBytes(packet.size); 
        //byte[] buffer2 = BitConverter.GetBytes(packet.packetId); 
        //Array.Copy(buffer,0,openSegment.Array, openSegment.Offset, buffer.Length);
        //Array.Copy(buffer,0,openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
        //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length+buffer2.Length);

        //Send(sendBuff);
        Thread.Sleep(5000);
        Disconnect();
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array,buffer.Offset);
        ushort id = BitConverter.ToUInt16(buffer.Array,buffer.Offset + 2);
        Console.WriteLine($"RecvPacketId :{id}, Size : {size}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
}
