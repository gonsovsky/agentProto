namespace AgentProto
{
    public class Config
    {
        public readonly int GramSize = Helper.SizeOf(typeof(ProtoGram));

        public readonly int BufferSize = 1024;

        public readonly int Delay = 1;

        public readonly int Port = 7777;

        public string Host = "localhost";

        public string RootFolder = @"C:\temp";
    }
}
