using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Corallite.Buffers;

namespace AgentProto
{
    public class Agent: ProtoParty
    {
        protected readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        public void Get(string uri, long start, long len, string file="")
        {
            if (file == "")
                file = uri;
            var state = new AgentState(Config, Fs)
            {
                FileName = file,
                Gram = new ProtoGram((byte)ProtoCommand.Get, start, len, uri)
            };
            try
            {
                var ipHostInfo = Dns.GetHostEntry(Config.Host);
                var ipAddress = ipHostInfo.AddressList
                    .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var remoteEp = new IPEndPoint(ipAddress, Config.Port);
                var client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                state.WorkSocket = client;
                client.BeginConnect(remoteEp, ConnectCallback, state);
                ConnectDone.WaitOne();
                Send(state);
                SendDone.WaitOne();
                Receive(state);
                AllDone.WaitOne();
                Complete(state);
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        public List<FsInfo> List(string uri)
        {
            return null;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var state = (AgentState)ar.AsyncState;
            try
            {
                state.WorkSocket.EndConnect(ar);
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        private void Send(AgentState state)
        {
            state.Send();
            state.WorkSocket.BeginSend(state.Buffer, 0, state.BufferLen, 0,
                SendCallback, state);
        }

        private void SendCallback(IAsyncResult ar)
        {
            var state = (AgentState)ar.AsyncState;
            try
            {
                var bytesSent = state.WorkSocket.EndSend(ar);
                SendDone.Set();
                OnRequest?.Invoke(this, state);
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        private void Receive(ProtoState state)
        {
            try
            {
                state.WorkSocket.BeginReceive(state.Buffer, 0, Config.BufferSize, 0,
                    ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (AgentState)ar.AsyncState;
            try
            {
                var client = state.WorkSocket;
                var bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.Receive(bytesRead);
                    client.BeginReceive(state.Buffer, 0, Config.BufferSize, 0,
                        ReceiveCallback, state);
                }
                else
                {
                    AllDone.Set();
                }
            }
            catch (Exception e)
            {
                Abort(state, e);
            }
        }

        public Agent(Config config, IFs fs) : base(config, fs)
        {
        }

        public override void Abort(ProtoState state, Exception e)
        {
            ConnectDone.Reset();
            SendDone.Reset();
            base.Abort(state, e);
        }
    }
}