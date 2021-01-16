using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AgentProto
{
    public struct ProtoGram
    {
        public byte Command;
        public byte Status;
        public long Start;
        public long Length;
        public short UrlLength;
        public byte[] UrlData;

        public ProtoGram(byte command, long start, long len, string uri)
        {
            Command = command;
            Start = start;
            Length = len;
            UrlLength = (short)uri.Length;
            UrlData = Encoding.UTF8.GetBytes(uri);
            Status = 0;
        }

        public int Size => Config.GramSize + UrlLength;

        public void ToByteArray(ref byte[] target)
        {
            using (var ms = new MemoryStream(target,true))
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(Command);
                    writer.Write(Status);
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
                    Command = reader.ReadByte(),
                    Status = reader.ReadByte(),
                    Start = reader.ReadInt64(),
                    Length = reader.ReadInt64(),
                    UrlLength = reader.ReadInt16()
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

    public enum ProtoStatus
    {
        Success = 0x1,
        Error = 0x2
    }
}
