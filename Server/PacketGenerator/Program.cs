using System.Xml;


XmlReaderSettings settings = new XmlReaderSettings()
{
    IgnoreComments = true,
    IgnoreWhitespace = true,
};
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

    ParseMembers(r);


}

void ParseMembers(XmlReader r)
{
    string packetName = r["name"];

    int depth = r.Depth + 1;

    while (r.Read())
    {
        if (r.Depth != depth)  // depth가 3일때는 어떡함? list의 멤버는?
            break;

        string memeberName = r["name"];
        if(string.IsNullOrEmpty (memeberName))
        {
            Console.WriteLine("Member without name");
            return;
        }

        string memberType = r.Name.ToLower();
        switch (memberType)
        {
            case "bool":
            case "byte":
            case "short":
            case "ushort":
            case "int":
            case "long":
            case "float":
            case "double":
            case "string":
            case "list":
                break;
            default:
                break;
        }
    }
}