using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Corallite.Buffers;

namespace AgentProto
{
    public abstract class ProtoState
    {
        protected Config Config;

        protected IFs Fs;

        protected ProtoState(Config config, IFs fs)
        {
            Config = config;
            Fs = fs;
            Buffer = UniArrayPool<byte>.Shared.Rent(Config.BufferSize);
        }

        public byte[] Buffer;

        public int BufferLen;

        public Stream File;

        public Socket WorkSocket;

        public ProtoGram Gram;

        public abstract bool Receive(int len);

        public virtual void Complete()
        {
            WorkSocket?.Shutdown(SocketShutdown.Both);
            WorkSocket?.Close();
            WorkSocket = null;
            if (Buffer != null)
                UniArrayPool<byte>.Shared.Return(Buffer);
            Buffer = null;
            if (File != null)
                Fs.Release(File);
            File = null;
            HeadRecv = false;
            HeadSent = false;
        }

        public virtual void Abort(Exception e)
        {
            Complete();
        }

        public bool HeadSent;

        public bool HeadRecv;

        public virtual void Send()
        {
            if (HeadSent)
                return;
            HeadSent = true;
            Gram.ToByteArray(ref Buffer);
            BufferLen = Gram.Size;
        }

        public string Url => Encoding.UTF8.GetString(Gram.UrlData);
    }
}
