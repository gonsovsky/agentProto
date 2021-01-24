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
            if (RecvStream != null)
                return false;
            switch (Gram.Command)
            {
                case ProtoCommand.Get:
                    RecvStream = Fs.Get(Url, Gram.Start, Gram.Length);
                    break;
                case ProtoCommand.List:
                    var list = Fs.List(Url);
                    RecvStream = list.SerializeStream();
                    break;
                case ProtoCommand.Head:
                    var head = Fs.Head(Url);
                    RecvStream = head.SerializeStream();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            FileLength = this.Gram.Length != 0 ? this.Gram.Length : RecvStream.Length - Gram.Start;
            return true;

        }

        public long FileLength;

        public bool HasSend()
        {
            return RecvStream.Position - Gram.Start < FileLength;
        }

        public override void Send()
        {
            BufferLen = 0;
            base.Send();
            if (Gram.Status == (byte) ProtoStatus.Success)
            {
                long size = Config.BufferSize - BufferLen;
                size = Math.Min(FileLength - RecvStream.Position + Gram.Start, size);
                BufferLen += RecvStream.Read(this.Buffer, BufferLen, (int) size);
            }
        }

        public void SendError(Exception e)
        {
            Gram.Status = ProtoStatus.Error;
        }

        public HubState(Config config, IFs fs) : base(config, fs)
        {
        }
    }
}
