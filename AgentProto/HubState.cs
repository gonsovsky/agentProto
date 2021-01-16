using System.IO;

namespace AgentProto
{
    public class HubState: ProtoState
    {
        public override bool Receive(int len)
        {
            BufferLen += len;
            if (BufferLen < Config.GramSize)
                return false;
            Gram = ProtoGram.FromByteArray(Buffer);
            File = Fs.Get(Url, Gram.Start, Gram.Length);
            FileLength = this.Gram.Length != 0 ? this.Gram.Length : File.Length;
      
            return true;
        }

        public long FileLength;

        public override void Send()
        {
            BufferLen = 0;
            base.Send();
            BufferLen += File.Read(this.Buffer, BufferLen, Config.BufferSize- BufferLen);
        }

        public HubState(Config config, IFs fs) : base(config, fs)
        {
        }
    }
}
