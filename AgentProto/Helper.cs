using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

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

        /*
        public static byte[] Serialize<T>(this T data)
            where T : struct
        {
            return data.SerializeStream().ToArray();
        }

        public static MemoryStream SerializeStream<T>(this T data)
            where T : struct
        {
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            formatter.Serialize(stream, data);
            return stream;
        }

        public static T Deserialize<T>(byte[] array)
            where T : struct
        {
            var stream = new MemoryStream(array);
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(stream);
        }*/
    }
}
