using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Earth.Extensions
{
    public static class SocketClient
    {
        private static readonly IPAddress IpAddress;
        private static readonly IPEndPoint RemoteEndPoint;
        private static readonly IPEndPoint LocalEndPoint;

        static SocketClient()
        {
            var host = Dns.GetHostEntry("localhost");
            IpAddress = host.AddressList[0];
            RemoteEndPoint = new IPEndPoint(IpAddress, 2002);
            LocalEndPoint = new IPEndPoint(IpAddress, 1918);
        }

        public static void SendContinuousMessages()
        {
            try
            {
                using var sender = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Bind(LocalEndPoint);
                sender.Connect(RemoteEndPoint);

                if (sender.Connected)
                {
                    Console.WriteLine($"Connected to Sun on {RemoteEndPoint} (Socket-Listener)..\n");
                }
                
                var counter = 1;
                while (true)
                {
                    var message = "Message# " + counter++;
                    var messageBytes = Encoding.ASCII.GetBytes(message);
                    sender.Send(messageBytes);
                    WriteSent(message);
                    var bytes = new byte[sender.ReceiveBufferSize];
                    var bytesReceived = sender.Receive(bytes);
                    var data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                    WriteReceived(data);
                    Console.WriteLine();
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Something went wrong while trying to create sender socket / connect to remote listener\n\tError content: " +
                    e.Message);
                throw;
            }
        }

        private static void WriteSent(string content)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\tSENT: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(content);
        }

        private static void WriteReceived(string content)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\tRECEIVED: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(content);
        }
    }
}