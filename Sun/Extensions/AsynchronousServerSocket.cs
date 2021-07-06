using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sun.Extensions
{
    public class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] Buffer { get; set; } = new byte[BufferSize];
        public Socket WorkSocket { get; set; } = null;
        public StringBuilder StringB { get; set; } = new StringBuilder();
    }

    public class AsynchronousServerSocket
    {
        private const int LocalPort = 2002;
        private readonly IPAddress _ipAddress;
        private readonly IPEndPoint _localEp;

        private static ManualResetEvent _allDone;

        public AsynchronousServerSocket()
        {
            _allDone = new ManualResetEvent(false);
            _ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            _localEp = new IPEndPoint(_ipAddress, LocalPort);
        }

        public void StartListening()
        {
            try
            {
                var listener = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(_localEp);
                listener.Listen(100);

                while (true)
                {
                    _allDone.Reset();
                    listener.BeginAccept(AcceptCallBack, listener);
                    _allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while trying to listen.\n\tError content: " + e.Message);
                throw;
            }
        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            _allDone.Set();

            var listener = (Socket) ar.AsyncState;
            var handler = listener.EndAccept(ar);

            var state = new StateObject {WorkSocket = handler};
            var buffer = state.Buffer;
            handler.BeginReceive(buffer, 0, StateObject.BufferSize, 0, ReadCallBack, state);
            Send(handler);
        }

        private static void ReadCallBack(IAsyncResult ar)
        {
            var state = (StateObject) ar.AsyncState;
            var handler = state.WorkSocket;

            var bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                var sb = state.StringB;
                var buffer = state.Buffer;

                sb.Append(Encoding.ASCII.GetString(buffer, 0, buffer.Length));
                var content = sb.ToString();
                Console.WriteLine($"Read data {content}");
            }
        }

        private static void Send(Socket handler)
        {
            var bytes = Encoding.ASCII.GetBytes(Console.ReadLine() ?? "Default message from server");
            handler.BeginSend(bytes, 0, bytes.Length, 0, SendCallBack, handler);
        }

        private static void SendCallBack(IAsyncResult ar)
        {
            var handler = (Socket) ar.AsyncState;
            var bytesSent = handler.EndSend(ar);
            Console.WriteLine($"Sent {bytesSent} bytes to client..");
        }
    }
}