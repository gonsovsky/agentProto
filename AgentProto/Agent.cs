using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AgentProto
{
    public class Agent
    {
        protected Config Config;

        protected Fs Fs;

        public Agent(Config config, Fs fs)
        {
            Config = config;
            Fs = fs;
        }

        protected readonly ManualResetEvent ConnectDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent SendDone =
            new ManualResetEvent(false);

        protected readonly ManualResetEvent ReceiveDone =
            new ManualResetEvent(false);

        protected string Response = String.Empty;

        public void Get(string uri, ulong start, ulong len, string file=null)
        {
            try
            {
                if (file == null)
                    file = uri;
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = ipHostInfo.AddressList[0];
                var remoteEp = new IPEndPoint(ipAddress, Config.Port);

                var client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                var state = new AgentState(Config, Fs) {WorkSocket = client, Gram = new ProtoGram(1, start, len, uri)};

                client.BeginConnect(remoteEp,
                    ConnectCallback, client);
                ConnectDone.WaitOne();

                Send(client, state.Gram);
                SendDone.WaitOne();

                Receive(state);
                ReceiveDone.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint);

                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, ProtoGram gram)
        {
            var byteData = gram.ToByteArray();

            client.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;

                var bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(AgentState state)
        {
            try
            {
                state.WorkSocket.BeginReceive(state.Buffer, 0, Config.BufferSize, 0,
                    ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var state = (AgentState)ar.AsyncState;
                var client = state.WorkSocket;

                var bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.Process();

                    client.BeginReceive(state.Buffer, 0, Config.BufferSize, 0,
                        ReceiveCallback, state);
                }
                else
                {
                    state.Complete();
                    ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}