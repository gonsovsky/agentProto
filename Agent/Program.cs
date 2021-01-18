using System;
using System.Threading;
using AgentProto;

namespace Agent
{
    internal class Program
    {
        private static void Main()
        {
            var agent = MakeAgent();
            agent.Get("123.txt", 0, 500000, "123-1.txt");

            agent = MakeAgent();
            agent.Get("123.txt", 500000, 0, "123-2.txt");

            Helper.Combine(Helper.AssemblyDirectory,
                new string[] {"123-1.txt", "123-2.txt"}, "123.txt");
            Console.ReadLine();
        }

        public static AgentProto.Agent MakeAgent()
        {
            var config = new Config { RootFolder = Helper.AssemblyDirectory };
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
                    Console.WriteLine(
                        $"Agent Abort   : {state.Url} [{state.Gram.Start}/{state.Gram.Length}] {ex.Message}");
                }
            };
            return agent;
        }
    }
}
