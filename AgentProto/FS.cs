using System.Collections.Generic;
using System.IO;

namespace AgentProto
{
    public class Fs
    {
        public Config Config;

        public Fs(Config config)
        {
            Config = config;
        }

        public Dictionary<string, Stream> Opened = new Dictionary<string, Stream>();

        public FileStream Get(string uri, ulong start, ulong length)
        {
            var file = Path.Combine(Config.RootFolder, uri);
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            stream.Seek((long)start, SeekOrigin.Begin);
            return stream;
        }

        public FileStream Put(string uri)
        {
            var file = Path.Combine(Config.RootFolder, uri);
            var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            return stream;
        }

        public void Release(string uri, FileStream fs)
        {
            fs?.Close();
        }
    }
}
