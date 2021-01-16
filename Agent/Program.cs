using System;
using AgentProto;

namespace Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config();
            config.RootFolder = @"C:\temp_get";
            var fs = new Fs(config);
            var agent = new AgentProto.Agent(config, fs);
            agent.Get("123.rar", 0, 0);
            Console.ReadLine();
        }
    }
}
