namespace AgentProto
{
    public class Config
    {
        public static readonly int GramSize = Helper.SizeOf(typeof(ProtoGram));

        /// <summary>
        /// Размер буфера должен вмещать протограмму с URL
        /// </summary>
        public readonly int BufferSize = 1024;

        public readonly int Port = 7777;

        public string Host = "localhost";

        public string RootFolder = @"C:\temp";
    }
}
