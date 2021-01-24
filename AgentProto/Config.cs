namespace AgentProto
{
    public class Config
    {
        public static readonly int GramSize = Helper.SizeOf(typeof(ProtoGram));

        /// <summary>
        /// Buffer size must be equal or greater than "Gram" with URL
        /// </summary>
        public readonly int BufferSize = 1024;

        public readonly int Port = 7777;

        public string Host = "localhost";

        public string RootFolder = Helper.AssemblyDirectory;
    }
}
