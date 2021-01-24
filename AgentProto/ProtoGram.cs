using System.IO;
using System.Text;

namespace AgentProto
{
    public struct ProtoGram
    {
        public ProtoCommand Command;
        public ProtoStatus Status;
        public long Start;
        public long Length;
        public short UrlLength;
        public byte[] UrlData;

        public ProtoGram(ProtoCommand command, long start, long len, string uri)
        {
            Command = command;
            Start = start;
            Length = len;
            UrlLength = (short)uri.Length;
            UrlData = Encoding.UTF8.GetBytes(uri);
            Status = ProtoStatus.Success;
        }

        public int Size => Config.GramSize + UrlLength;

        public string Url
        {
            get => Encoding.UTF8.GetString(UrlData);
            set
            {
                UrlLength = (short)value.Length;
                UrlData = Encoding.UTF8.GetBytes(value);
            }
        }


        public void ToByteArray(ref byte[] target)
        {
            using (var ms = new MemoryStream(target,true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((byte)Command);
                    writer.Write((byte)Status);
                    writer.Write(Start);
                    writer.Write(Length);
                    writer.Write(UrlLength);
                    writer.Write(UrlData);
                }
            }
        }

        public static ProtoGram FromByteArray(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                var res = new ProtoGram
                {
                    Command = (ProtoCommand) reader.ReadByte(),
                    Status = (ProtoStatus) reader.ReadByte(),
                    Start = reader.ReadInt64(),
                    Length = reader.ReadInt64(),
                    UrlLength = reader.ReadInt16()
                };
                res.UrlData = reader.ReadBytes(res.UrlLength);
                return res;
            }
        }
    }

    public enum ProtoCommand: byte
    {
        Get = 0x1,
        List = 0x2,
        Head = 0x3
    }

    public enum ProtoStatus: byte
    {
        Success = 0x0,
        Error = 0x1
    }
}
