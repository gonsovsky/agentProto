using System.IO;

namespace AgentProto
{
    public class AgentState: ProtoState
    {
        public AgentState(Config config, Fs fs) : base(config, fs)
        {
        }

        public override bool Receive(byte[] data, int len)
        {
            if (File == null)
            {
                File = Fs.Put(Gram.Url);
            }
            File.Write(data, 0, len);
            return true;
        }
    }
}
