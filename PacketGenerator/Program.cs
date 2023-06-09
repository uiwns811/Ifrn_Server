using PacketGenerator;
using System;
using System.Xml;

// batchfile : 윈도우에서 제공해주는 명령어들을 작성해서 한 번에 실행하도록 해주는 개념 
// PacketGenerator의 프로젝트 폴더 내 exe 를 클릭해주는 작업


namespace PacketgGenerator
{
    class Program
    {
        static string getPacket;
        static ushort packetid;
        static string packetEnums;

        static void Main(string[] args) 
        {
            string pdlPath = "../PDL.xml";
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            if (args.Length >= 1)
                pdlPath = args[0];

            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                r.MoveToContent();
                // xml에서 헤더 건너뛰고 컨텐츠 (정의한 부분)으로 바로 감

                while(r.Read())
                {
                    // depth : 0부터 몇 개로 파고드는지. packet = 1
                    // Element : parse 시작 (<packet name = ~ >)
                    // EndElement : parse 종료 (</packet>)
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                    Console.WriteLine(r.Name + " " + r["name"]);
                }

                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, getPacket);
                File.WriteAllText("GenPackets.cs", fileText);
            }
            // using 범위 벗어나면 자동으로 Dispose 해줌
        }

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid Packet Load");
                return;
            }
            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet Without Name");
                return; 
            }

            Tuple<string, string, string> t = ParseMembers(r);
            getPacket += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3) + Environment.NewLine;
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetid)
                + Environment.NewLine + "\t";      // 엔터 후 tab
        }

        // {0} : 패킷 이름
        // {1} : 멤버 변수
        // {2} : 멤버 변수 Read
        // {3} : 멤버 변수 Write
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1;        // packet의 depth + 1 (packet 내부에 있는 playerid, name, skills)
            while(r.Read())
            {
                if (r.Depth != depth)
                    break;

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;          // 엔터 침

                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;           // 엔터 침

                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;          // 엔터 침

                string memberType = r.Name.ToLower();
                switch(memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long": 
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
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
            memberCode = memberCode.Replace("\n", "\n\t");          // 엔터 있는 애들은 엔터 + 탭 
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName)) 
            {
                Console.WriteLine("List Without Name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.memberListFormat,
                FirstCharToUpper(listName), FirstCharToLower(listName),
                t.Item1, t.Item2, t.Item3);

            string readCode = string.Format(PacketFormat.readListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));
            string writeCode = string.Format(PacketFormat.writeListFormat, FirstCharToUpper(listName), FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)
        {
            switch(memberType)
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

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
            // - input의 첫번째 글자만 대문로 바꾼 것 + 1번째부터 문자열 자른 것
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
            // - input의 첫번째 글자만 대문로 바꾼 것 + 1번째부터 문자열 자른 것
        }
    }
}