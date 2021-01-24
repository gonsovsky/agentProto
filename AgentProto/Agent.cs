using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AgentProto
{
    public class Agent: ProtoParty
    {
        protected readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        public object Cmd(ProtoCommand cmd, string uri, long start, long len, string file = "")
        {
            if (file == "")
                file = uri;
            var state = new AgentState(Config, Fs)
            {
                FileName = file,
                Gram = new ProtoGram(cmd, start, len, uri)
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
                if (state.Gram.Status == ProtoStatus.Success )
                    return Complete(state);
                else
                {
                    throw new ApplicationException(state.Url);
                }
            }
            catch (Exception e)
            {
                Abort(state, e);
                return null;
            }
        }

        public bool Get(string uri, long start, long len, string file="")
        {   
            var obj = Cmd(ProtoCommand.Get, uri, start, len, file);
            if (obj!=null)
                return (bool) obj;
            return false;
        }

        public FsInfo[] List(string uri)
        {
            var obj = Cmd(ProtoCommand.List, uri, 0, 0, null);
            return (FsInfo[]) obj;
        }

        public FsInfo Head(string uri)
        {
            var obj = Cmd(ProtoCommand.Head, uri, 0, 0, null);
            return (FsInfo) obj;
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
                state.WorkSocket.EndSend(ar);
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

        public override object Complete(ProtoState state)
        {
            ConnectDone.Reset();
            SendDone.Reset();
            return base.Complete(state);
        }
    }
}