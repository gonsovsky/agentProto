using System;
using System.Net;
using System.Net.Sockets;
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
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
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

                    Console.WriteLine("Waiting for a connection...");
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

            Console.WriteLine("\nPress ENTER to continue...");
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

            state.Recv.Write(state.Buffer,0, bytesRead);
                
            if (state.Process())
            {
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                Send(state);
            }
            else
            {
                handler.BeginReceive(state.Buffer, 0, Config.BufferSize, 0,
                    ReadCallback, state);
            }
        }

        protected void Send(HubState state)
        {
            var bytes = new byte[state.Send.Length];
            state.Send.Read(bytes, 0, bytes.Length);

            state.WorkSocket.BeginSend(bytes, 0, bytes.Length, 0,
                SendCallback, state.WorkSocket);
        }

        protected void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = (Socket) ar.AsyncState;

                var bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}