using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace AgentProto
{
    public static class Helper
    {
        public static int SizeOf(Type t)
        {
            var fields = t
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            return fields.Select(f => f.FieldType)
                .Select(x => x.IsPrimitive ? Marshal.SizeOf(x) : SizeOf(x)).Sum();
        }

        public static void Combine(string inputDirectoryPath, string[] inputFilePaths, string outputFilePath)
        {
            using (var outputStream = File.Create(outputFilePath))
            {
                foreach (var inputFilePath in inputFilePaths)
                {
                    using (var inputStream = File.OpenRead(inputFilePath))
                    {
                        inputStream.CopyTo(outputStream);
                    }
                }
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static byte[] Serialize<T>(this T data)
            where T : struct
        {
            return data.SerializeStream().ToArray();
        }

        public static MemoryStream SerializeStream<T>(this T data)
        
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            stream.Position = 0;
            return stream;
        }

        public static T Deserialize<T>(byte[] array)
        {
            var stream = new MemoryStream(array);
            return DeserializeStream<T>(stream);
        }

        public static T DeserializeStream<T>(Stream stream)
        
        {
            stream.Position = 0;
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(stream);
        }
    }
}
