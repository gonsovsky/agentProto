using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AgentProto
{
    public interface IFs
    {
        Stream Get(string uri, long start, long length);
        Stream Put(string filename);
        void Release(Stream fs);
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

        public Stream Get(string uri, long start, long length)
        {
            var file = Path.Combine(Config.RootFolder, uri);
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            stream.Seek((long)start, SeekOrigin.Begin);
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
             Random rnd = new Random();
             Byte[] b = new Byte[sizeInKb];
             for (int i = 0; i < b.Length; i++)
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

        public IEnumerable<FsInfo> List(string dir)
        {
            throw new System.NotImplementedException();
        }
    }

}
