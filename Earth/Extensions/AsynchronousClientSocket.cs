using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Earth.Extensions
{
    public class StateObject
    {
        public const int BufferSize = 256;
        public byte[] Buffer { get; set; } = new byte[BufferSize];
        public Socket WorkSocket { get; set; } = null;
        public StringBuilder StringB { get; set; } = new StringBuilder();
    }

    public class AsynchronousClientSocket
    {
        private const int RemotePort = 2002;
        private readonly IPAddress _ipAddress;
        private readonly IPEndPoint _remoteEp;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent _connectDone;
        private static ManualResetEvent _sendDone;
        private static ManualResetEvent _receiveDone;

        public AsynchronousClientSocket()
        {
            _connectDone = _sendDone = _receiveDone = new ManualResetEvent(false);
            _ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            _remoteEp = new IPEndPoint(_ipAddress, RemotePort);
        }

        public void StartClient()
        {
            try
            {
                using var client = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(_remoteEp, ConnectCallBack, client);
                _connectDone.WaitOne();

                while (true)
                {
                    var message = Console.ReadLine() ?? "Default message!";
                    Send(client, message);
                    _sendDone.WaitOne();

                    Receive(client);
                    _receiveDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Something went wrong while trying to communicate with socket server.\n\tError content: " +
                    e.Message);
                throw;
            }
        }

        private static void ConnectCallBack(IAsyncResult ar)
        {
            var client = (Socket) ar.AsyncState;
            client.EndConnect(ar);
            Console.WriteLine($"Client socket connected to the {client.RemoteEndPoint}.");
            _connectDone.Set();
        }

        public void Send(Socket client, string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            client.BeginSend(bytes, 0, bytes.Length, 0, SendCallBack, client);
            _sendDone.WaitOne();
        }

        private static void SendCallBack(IAsyncResult ar)
        {
            var client = (Socket) ar.AsyncState;
            var bytesSent = client.EndSend(ar);
            Console.WriteLine($"Sent {bytesSent} bytes to remote server..");
            _sendDone.Set();
        }

        public void Receive(Socket client)
        {
            var state = new StateObject {WorkSocket = client};
            var buffer = state.Buffer;
            client.BeginReceive(buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject) ar.AsyncState;
            var client = state.WorkSocket;

            var bytesRead = client.EndReceive(ar);
            var buffer = state.Buffer;
            var sb = state.StringB;
            if (bytesRead > 0)
            {
                sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                client.BeginReceive(buffer, 0, StateObject.BufferSize, 0,
                    ReceiveCallback, state);
            }
            else
            {
                if (sb.Length > 1)
                {
                    Console.WriteLine(sb.ToString());
                }

                _receiveDone.Set();
            }
        }
    }
}