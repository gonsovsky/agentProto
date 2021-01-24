using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgentProto
{
    public interface IFs
    {
        Stream Get(string uri, long start, long length);
        Stream Put(string filename);
        void Release(Stream fs);
        FsInfo[] List(string dir);
        FsInfo Head(string uri);
    }

    [Serializable]
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

        public Stream Get(string uri, long start, long length)
        {
            var file = Path.Combine(Config.RootFolder, uri);
            var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            stream.Seek(start, SeekOrigin.Begin);
            return stream;
        }

        public Stream Put(string filename)
        {
            var file = Path.Combine(Config.RootFolder, filename);
            var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            return stream;
        }

        public void Release(Stream fs)
        {
            fs?.Close();
        }

        public FsInfo[] List(string dir)
        {
            var dirX = PathX(dir);
            return Directory.GetFiles(dirX)
                .Select(x => new FsInfo()
                {

                    Name = Path.GetFileName(x),
                    Size = new FileInfo(Path.Combine(dirX, x)).Length
                }).ToArray();
        }

        public FsInfo Head(string uri)
        {
            var dirX = PathX(uri);
            return new FsInfo()
            {
                Name = uri,
                Size = new FileInfo(dirX).Length
            };
        }

        public string PathX(string uri)
        {
            while (true)
            {
                String newPath = new Regex(@"[^\\/]+(?<!\.\.)[\\/]\.\.[\\/]").Replace(uri, "");
                if (newPath == uri) break;
                uri = newPath;
            }
            uri = uri.Replace("/", Path.DirectorySeparatorChar.ToString());
            uri = uri.Replace(@"\", Path.DirectorySeparatorChar.ToString());
            var dirX = Path.Combine(Config.RootFolder, uri);
            return dirX;
        }
    }

    public class StubFs : IFs
    {
        public Config Config;

        public StubFs(Config config)
        {
            Config = config;
        }

        public static MemoryStream Data = new MemoryStream(GetByteArray(1000000));

        private static byte[] GetByteArray(int sizeInKb)
         {
             var b = new byte[sizeInKb];
             for (var i = 0; i < b.Length; i++)
             {
                b[i] = 55;
             }
             return b;
         }

        public Stream Get(string uri, long start, long length)
        {
            Data.Seek(start, SeekOrigin.Begin);
            return Data;
        }

        public Stream Put(string filename)
        {
            var file = Path.Combine(Config.RootFolder, filename);
            var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            return stream;
        }

        public void Release(Stream fs)
        {
            //fs?.Close();
        }

        public FsInfo[] List(string dir)
        {
            return new[] { new FsInfo() {Name = "123.txt", Size = new FileInfo("123.txt").Length}};
        }

        public FsInfo Head(string uri)
        {
            return new FsInfo() {Name = "123.txt", Size = new FileInfo("123.txt").Length};
        }
    }

}
