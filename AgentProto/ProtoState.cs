using System;
using System.IO;
using System.Net.Sockets;
using Corallite.Buffers;

namespace AgentProto
{
    public abstract class ProtoState
    {
        protected Config Config;

        protected Fs Fs;

        protected ProtoState(Config config, Fs fs)
        {
            Config = config;
            Fs = fs;
            Buffer = UniArrayPool<byte>.Shared.Rent(Config.BufferSize);
        }

        public byte[] Buffer;

        public FileStream File;

        public Socket WorkSocket = null;

        public ProtoGram Gram;

        public abstract bool Receive(byte[] data, int len);

        public bool Complete()
        {
            Fs.Release(Gram.Url, File);
            Console.WriteLine("Complete: " + Gram.Url);
            return true;
        }
    }
}
