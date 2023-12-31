using ServerCore;
using System.Net;
using System.Text;

public enum PacketID
{
    PlayerInfoReq = 1,
	Test = 2,
	
}

interface IPacket
{
	ushort Protocol { get; }
	void Read(ArraySegment<byte> segment);
	ArraySegment<byte> Write();
}


class PlayerInfoReq : IPacket
{
    public byte testByte;
	public long playerId;
	public string name;
	public struct Skill
	{
	    public int id;
		public short level;
		public float duration;
	    public void Read(ReadOnlySpan<byte> s, ref ushort count)
	    {
	        id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
			count += sizeof(short);
			duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
	    }
	
	    public bool Write(Span<byte> s, ref ushort count)
	    {
	        bool success = true;
	        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
			count += sizeof(short);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
			count += sizeof(float);
	        return success;
	    }
	}
	
	public List<Skill> skills = new List<Skill>();

    public ushort Protocol { get { return (ushort)PacketID.PlayerInfoReq; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
		playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
		count += sizeof(long);
		ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
		count += nameLen;
		skills.Clear();
		ushort skillLen =BitConverter.ToUInt16(s.Slice(count, s.Length-count));
		count += sizeof(ushort);
		for(int i =0; i< skillLen; i++)
		{
		    Skill skill = new Skill();
		    skill.Read(s, ref count);
		    skills.Add(skill);
		}
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
        count += sizeof(ushort);
        segment.Array[segment.Offset + count] = (byte)this.testByte;
		count += sizeof(byte);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), playerId);
		count += sizeof(long);
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(name, 0, name.Length, segment.Array, segment.Offset + count+sizeof(ushort) );
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
		count += sizeof(ushort);
		count += nameLen;
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
		count += sizeof(ushort);
		foreach(Skill skill in skills)
		    success &= skill.Write(s, ref count);
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

class Test : IPacket
{
    public int abc;

    public ushort Protocol { get { return (ushort)PacketID.Test; } }

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;

        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        abc = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.Test);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), abc);
		count += sizeof(int);
        success &= BitConverter.TryWriteBytes(s, count);
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

