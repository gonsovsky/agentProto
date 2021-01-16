using System.IO;

namespace AgentProto
{
    public class AgentState: ProtoState
    {
        public AgentState(Config config, Fs fs) : base(config, fs)
        {
        }

        public override bool Process()
        {
            if (RecvFile == null)
            {
                RecvFile = Fs.Put(Gram.Url);
            }
            RecvFile.Write(this.Buffer);
            return true;
        }

        public override bool Complete()
        {
            Fs.Release(Gram.Url, RecvFile);
            return true;
        }

        public FileStream RecvFile;

    }
}
