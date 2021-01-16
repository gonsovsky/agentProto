using System.IO;

namespace AgentProto
{
    public class AgentState: ProtoState
    {
        public AgentState(Config config, IFs fs) : base(config, fs)
        {

        }

        public override bool Receive(int len)
        {
            var headDelta=0;
            if (!HeadRecv)
            {
                HeadRecv = true;
                if (BufferLen < Config.GramSize)
                    return false;
                Gram = ProtoGram.FromByteArray(Buffer);
                headDelta = Gram.Size;
            }
            if (File == null)
            {
                File = Fs.Put(FileName);
            }
            File.Write(Buffer, headDelta, len- headDelta);
            return true;
        }

        public string FileName;

    }
}
