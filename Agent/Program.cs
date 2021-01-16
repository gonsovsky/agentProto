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
            var agent = new AgentProto.Agent(config, fs)
            {
                OnRequest = (party, state) =>
                {
                    Console.WriteLine($"Agent Request : {state.Url} [{state.Gram.Start}/{state.Gram.Length}]");
                },
                OnResponse = (party, state) =>
                {
                    Console.WriteLine($"Agent Response: {state.Url} [{state.Gram.Start}/{state.Gram.Length}]");
                },
                OnAbort = (party, state, ex) =>
                {
                    Console.WriteLine($"Agent Abort   : {state.Url} [{state.Gram.Start}/{state.Gram.Length}] {ex.Message}");
                }
            };
            agent.Get("TeamViewer_Setup.exe", 0, 500000, "123-1.txt");
            Console.ReadLine();
        }
    }
}
