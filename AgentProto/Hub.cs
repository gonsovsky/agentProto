using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AgentProto
{
    public class Hub
    {
        protected Config Config;

        protected Fs Fs;

        protected ManualResetEvent AllDone = new ManualResetEvent(false);

        public Hub(Config config, Fs fs)
        {
            Config = config;
            Fs = fs;
        }

        public void StartListening()
        {
            var ipHostInfo = Dns.GetHostEntry(Config.Host);
            var ipAddress = ipHostInfo.AddressList
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            var localEndPoint = new IPEndPoint(ipAddress, Config.Port );

            var listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
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
                Console.WriteLine(e.ToString());
            }
            Console.Read();
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
            var content = string.Empty;

            var state = (HubState) ar.AsyncState;
            var handler = state.WorkSocket;

            var bytesRead = handler.EndReceive(ar);

            if (bytesRead <= 0)
                return;

            if (state.Receive(state.Buffer, bytesRead))
            {
                Send(state);
            }
            else
            {
                handler.BeginReceive(state.Buffer, 0, Config.BufferSize, 0,
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
            try
            {
                var state = (HubState) ar.AsyncState;

                var bytesSent = state.WorkSocket.EndSend(ar);

                if (state.File.Position < state.File.Length)
                {
                    if (Config.Delay!=0)
                        Thread.Sleep(Config.Delay);
                    Send(state);
                }
                else
                {
                    state.Complete();
                    state.WorkSocket.Shutdown(SocketShutdown.Both);
                    state.WorkSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}