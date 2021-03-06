﻿using System;
using AgentProto;

namespace Hub
{
    internal class Program
    {
        private static void Main()
        {
            var config = new Config();
            var fs = new Fs(config);
            var hub = new AgentProto.Hub(config, fs)
            {
                OnRequest = (party, state) =>
                {
                    Console.WriteLine($"Hub Request : {state.Url} [{state.Gram.Start}/{state.Gram.Length}]");
                },
                OnResponse = (party, state) =>
                {
                    Console.WriteLine($"Hub Response: {state.Url} [{state.Gram.Start}/{state.Gram.Length}]");
                },
                OnAbort = (party, state, ex) =>
                {
                    Console.WriteLine($"Hub Abort   : {ex.Message}");
                }
            };
            hub.Listen();
        }
    }
}
