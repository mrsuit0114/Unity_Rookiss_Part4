using PacketGenerator;
using System.Xml;


XmlReaderSettings settings = new XmlReaderSettings()
{
    IgnoreComments = true,
    IgnoreWhitespace = true,
};

string genPackets="";

// 나중에 Dispose로 닫거나 using을 사용해서 해당 부분에서만 사용하도록 하던가 -> 자동닫기인지는 모르겠지만 비슷하게 작용
using (XmlReader r = XmlReader.Create("PDL.xml", settings))  // exe파일 생성위치에서 찾으므로 일단은 bin -> .. ->exe있는곳에 xml복붙
{
    r.MoveToContent();  // 헤더 건너뛰고 여기에서는 <packet>으로 바로 들어간다고함

    while (r.Read())
    {
        if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)  // 여는 태그인지 확인, 여기서 <packet>은 depth가 1임
            ParsePacket(r);

        //Console.WriteLine(r.Name + " " + r["name"]);  // packet PlayerInfoReq...
    }

    File.WriteAllText("GenPackets.cs", genPackets);

}

void ParsePacket(XmlReader r)
{
    if (r.NodeType == XmlNodeType.EndElement)  // 닫는 태그인지 확인
        return;
    if (r.Name.ToLower() != "packet")  // packet태그인지 확인
        return;

    string packetName = r["name"];
    if(string.IsNullOrEmpty(packetName))
    {
        Console.WriteLine("Packet without name");
        return;
    }

    Tuple<string,string,string> t = ParseMembers(r);
    genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1,t.Item2,t.Item3);

}

// {1} 멤버 변수들
// {2} 멤버 변수 Read
// {3} 멤버 변수 Write
Tuple<string,string,string> ParseMembers(XmlReader r)
{
    string packetName = r["name"];

    string memberCode = "";
    string readCode = "";
    string writeCode = "";

    int depth = r.Depth + 1;

    while (r.Read())
    {
        if (r.Depth != depth)  // depth가 3일때는 어떡함? list의 멤버는?
            break;

        string memeberName = r["name"];
        if(string.IsNullOrEmpty (memeberName))
        {
            Console.WriteLine("Member without name");
            return null;
        }

        if (string.IsNullOrEmpty(memberCode) == false)
            memberCode += Environment.NewLine;  // enter치는 것과 동일하다고함
        if (string.IsNullOrEmpty(readCode) == false)
            readCode += Environment.NewLine;  // enter치는 것과 동일하다고함
        if (string.IsNullOrEmpty(writeCode) == false)
            writeCode += Environment.NewLine;  // enter치는 것과 동일하다고함

        string memberType = r.Name.ToLower();
        switch (memberType)
        {
            case "bool":
            case "short":
            case "ushort":
            case "int":
            case "long":
            case "float":
            case "double":
                memberCode += string.Format(PacketFormat.memberFormat, memberType, memeberName);
                readCode += string.Format(PacketFormat.readFormat, memeberName, ToMemberType(memberType), memberType);
                writeCode += string.Format(PacketFormat.writeFormat, memeberName, memberType);
                break;
            case "string":
                memberCode += string.Format(PacketFormat.memberFormat, memberType, memeberName);
                readCode += string.Format(PacketFormat.readStringFormat, memeberName);
                writeCode += string.Format(PacketFormat.writeStringFormat, memeberName);
                break;
            case "list":
                Tuple<string, string, string> t = ParseList(r);
                memberCode += t.Item1;
                readCode += t.Item2;
                writeCode += t.Item3;
                break;
            default:
                break;
        }
    }
    memberCode = memberCode.Replace("\n", "\n\t");
    readCode = readCode.Replace("\n", "\n\t\t");
    writeCode = writeCode.Replace("\n", "\n\t\t");
    return new Tuple<string, string, string>(memberCode,readCode,writeCode);
}

Tuple<string, string, string> ParseList(XmlReader r)
{
    string listName = r["name"];
    if (string.IsNullOrEmpty(listName))
    {
        Console.WriteLine("List without name");
        return null;
    }

    Tuple<string, string, string> t = ParseMembers(r);

    string memberCode = string.Format(PacketFormat.memberListFormat,
        FirstCharToUpper(listName),
        FirstCharToLower(listName),
        t.Item1,
        t.Item2,
        t.Item3);

    string readCode = string.Format(PacketFormat.readListFormat,
        FirstCharToUpper(listName),
        FirstCharToLower(listName));

    string writeCode = string.Format(PacketFormat.writeListFormat,
        FirstCharToUpper(listName),
        FirstCharToLower(listName));

    return new Tuple<string, string, string>(memberCode ,readCode, writeCode);

}

string FirstCharToUpper(string input){
    if (string.IsNullOrEmpty(input))
        return "";
    return input[0].ToString().ToUpper() + input.Substring(1);
}

string FirstCharToLower(string input)
{
    if (string.IsNullOrEmpty(input))
        return "";
    return input[0].ToString().ToLower() + input.Substring(1);
}

string ToMemberType(string memberType)
{
    switch (memberType)
    {
        case "bool":
            return "ToBoolean";
        case "short":
            return "ToInt16";
        case "ushort":
            return "ToUInt16";
        case "int":
            return "ToInt32";
        case "long":
            return "ToInt64";
        case "float":
            return "ToSingle";
        case "double":
            return "ToDouble";
        default:
            return "";
    }
}