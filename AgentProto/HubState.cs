using System.IO;

namespace AgentProto
{
    public class HubState: ProtoState
    {
        public MemoryStream Recv = new MemoryStream();

        public override bool Receive(byte[] data, int len)
        {
            Recv.Write(Buffer, 0, len);
            Recv.Position = 0;
            try
            {
                if (Recv.Length >= Config.GramSize)
                {
                    Gram = ProtoGram.FromStream(Recv);
                    if (Gram.Ready)
                    {
                        File = Fs.Get(Gram.Url, Gram.Start, Gram.Length);
                        Recv = new MemoryStream();
                        return true;
                    }
                }
            }
            finally
            {
                Recv.Position = Recv.Length;
            }
            
            return false;
        }

        public void Send()
        {
            this.Buffer = new byte[Config.BufferSize];
            BufferLen = File.Read(this.Buffer, 0, Config.BufferSize);
        }

        public HubState(Config config, Fs fs) : base(config, fs)
        {
        }

        public int BufferLen;
    }
}
