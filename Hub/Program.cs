using AgentProto;

namespace Hub
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config();
            var fs = new Fs(config);
            var hub = new AgentProto.Hub(config, fs);
     
           hub.StartListening();

        
        }
    }
}
