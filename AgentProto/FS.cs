using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AgentProto
{
    public interface IFs
    {
        FileStream Get(string uri, long start, long length);
        FileStream Put(string filename);
        void Release(FileStream fs);
        IEnumerable<FsInfo> List(string dir);
    }

    public struct FsInfo
    {
        public string Name;
        public long Size;
    }

    public class Fs: IFs
    {
        public Config Config;

        public Fs(Config config)
        {
            Config = config;
        }

        public FileStream Get(string uri, long start, long length)
        {
            var file = Path.Combine(Config.RootFolder, uri);
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            stream.Seek((long)start, SeekOrigin.Begin);
            return stream;
        }

        public FileStream Put(string filename)
        {
            var file = Path.Combine(Config.RootFolder, filename);
            var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            return stream;
        }

        public void Release(FileStream fs)
        {
            fs?.Close();
        }

        public IEnumerable<FsInfo> List(string dir)
        {
            var dirX = Path.Combine(Config.RootFolder, dir);
            return Directory.GetFiles(dirX)
                .Select(x => new FsInfo()
                {
                    Name = x,
                    Size = new FileInfo(Path.Combine(dirX, x)).Length
                });
        }
    }
}
