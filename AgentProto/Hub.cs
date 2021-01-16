using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AgentProto
{
    public class Hub: ProtoParty
    {
        public void Listen()
        {
            try
            {
                var ipHostInfo = Dns.GetHostEntry(Config.Host);
                var ipAddress = ipHostInfo.AddressList
                    .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                var localEndPoint = new IPEndPoint(ipAddress, Config.Port);
                var listener = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    AllDone.Reset();
                    listener.BeginAccept(
                        AcceptCallback,
                        listener);
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Abort(null, e);
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            AllDone.Set();
            var listener = (Socket) ar.AsyncState;
            var handler = listener.EndAccept(ar);
            var state = new HubState(Config, Fs)
            {
                WorkSocket = handler
            };
            handler.BeginReceive(state.Buffer, 0, Config.BufferSize, 0,
                ReadCallback, state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var state = (HubState) ar.AsyncState;
            var bytesRead = state.WorkSocket.EndReceive(ar);
            if (bytesRead <= 0)
                return;
            if (state.Receive(bytesRead))
            {
                OnRequest?.Invoke(this, state);
                Send(state);
            }
            else
            {
                state.WorkSocket.BeginReceive(state.Buffer, state.BufferLen, Config.BufferSize, 0,
                    ReadCallback, state);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Send(HubState state)
        {
            state.Send();
            state.WorkSocket.BeginSend(state.Buffer, 0, state.BufferLen, 0,
                SendCallback, state);
        }

        protected void SendCallback(IAsyncResult ar)
        {
            var state = (HubState)ar.AsyncState;
            try
            {
                var bytesSent = state.WorkSocket.EndSend(ar);

                if (state.File.Position - state.Gram.Start < state.FileLength)
                {
                    Send(state);
                }
                else
                {
                    Complete(state);
                }
            }
            catch (Exception e)
            {   
                Abort(state, e);
            }
        }

        public Hub(Config config, Fs fs) : base(config, fs)
        {
        }
    }
}