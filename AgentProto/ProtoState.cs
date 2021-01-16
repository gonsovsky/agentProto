using System.Net.Sockets;
namespace AgentProto
{
    public abstract class ProtoState
    {
        protected Config Config;

        protected Fs Fs;

        public ProtoState(Config config, Fs fs)
        {
            Config = config;
            Fs = fs;
            Buffer = new byte[Config.BufferSize];
  
            
        }

        public byte[] Buffer;

        public Socket WorkSocket = null;

        public ProtoGram Gram;

        public abstract bool Process();

        public abstract bool Complete();
    }
}
