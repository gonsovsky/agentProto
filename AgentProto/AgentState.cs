using System;
using System.IO;

namespace AgentProto
{
    public class AgentState: ProtoState
    {
        public AgentState(Config config, IFs fs) : base(config, fs)
        {
            Result = null;
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
            if (RecvStream == null)
            {
                switch (Gram.Command)
                {
                    case ProtoCommand.List:
                    case ProtoCommand.Head:
                        RecvStream = new MemoryStream();
                        break;
                    case ProtoCommand.Get:
                        RecvStream = Fs.Put(FileName);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            RecvStream?.Write(Buffer, headDelta, len - headDelta);
            return true;
        }

        public override object Complete()
        {
            if (RecvStream == null)
                return base.Complete();
            switch (Gram.Command)
            {
                case ProtoCommand.List:
                    if (Gram.Status == ProtoStatus.Success)
                        Result = Helper.DeserializeStream<FsInfo[]>(RecvStream);
                    RecvStream.Close();
                    break;
                case ProtoCommand.Head:
                    if (Gram.Status == ProtoStatus.Success)
                        Result = Helper.DeserializeStream<FsInfo>(RecvStream);
                    RecvStream.Close();
                    break;
                case ProtoCommand.Get:
                    Fs.Release(RecvStream);
                    Result = (Gram.Status == ProtoStatus.Success);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return base.Complete();
        }

        public string FileName;

    }
}
