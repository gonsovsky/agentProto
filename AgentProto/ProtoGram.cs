using System.IO;
using System.Text;

namespace AgentProto
{
    public struct ProtoGram
    {
        public byte Command;
        public byte Status;
        public ulong Start;
        public ulong Length;
        public ushort UrlLength;
        public byte[] UrlData;

        public string Url => Encoding.UTF8.GetString(UrlData);

        public bool Ready => Status == 0;

        public ProtoGram(byte command, ulong start, ulong len, string uri)
        {
            Command = command;
            Start = start;
            Length = len;
            UrlLength = (ushort)uri.Length;
            UrlData = Encoding.UTF8.GetBytes(uri);
            Status = 0;
        }

        public MemoryStream ToStream()
        {
            var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(Command);
                writer.Write(Status);
                writer.Write(Start);
                writer.Write(Length);
                writer.Write(UrlLength);
                writer.Write(UrlData);
            }
            return ms;
        }

        public byte[] ToByteArray()
        {
            return ToStream().ToArray();
        }

        public static ProtoGram FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var res = new ProtoGram
                {
                    Command = reader.ReadByte(),
                    Status = reader.ReadByte(),
                    Start = reader.ReadUInt64(),
                    Length = reader.ReadUInt64(),
                    UrlLength = reader.ReadUInt16()
                };
                res.UrlData = reader.ReadBytes(res.UrlLength);
                return res;
            }
        }
    }

    public enum ProtoCommand
    {
        Get = 0x1,
        List = 0x2
    }
}
