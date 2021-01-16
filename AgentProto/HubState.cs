using System.IO;

namespace AgentProto
{
    public class HubState: ProtoState
    {
        public MemoryStream Recv = new MemoryStream();

        public FileStream Send;

        public override bool Process()
        {
            Recv.Position = 0;
            try
            {
                if (Recv.Length >= Config.GramSize)
                {
                    Gram = ProtoGram.FromStream(Recv);
                    if (Gram.Ready)
                    {
                        Send = Fs.Get(Gram.Url, Gram.Start, Gram.Length);
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

        public override bool Complete()
        {
            return true;
        }

        public HubState(Config config, Fs fs) : base(config, fs)
        {
        }
    }
}
