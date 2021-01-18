using System;

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
            FileLength = this.Gram.Length != 0 ? this.Gram.Length : File.Length - Gram.Start;
      
            return true;
        }

        public long FileLength;

        public bool HasSend()
        {
            return File.Position - Gram.Start < FileLength;
        }

        public override void Send()
        {
            BufferLen = 0;
            base.Send();
            long size = Config.BufferSize - BufferLen;
            size = Math.Min(FileLength - File.Position + Gram.Start, size);
            BufferLen += File.Read(this.Buffer, BufferLen, (int)size);
        }

        public HubState(Config config, IFs fs) : base(config, fs)
        {
        }
    }
}
