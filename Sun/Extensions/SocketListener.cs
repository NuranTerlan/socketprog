using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sun.Extensions
{
    public static class SocketListener
    {
        private static readonly IPAddress IpAddress;
        private static readonly IPEndPoint LocalEndPoint;

        static SocketListener()
        {
            var host = Dns.GetHostEntry("localhost");
            IpAddress = host.AddressList[0];
            LocalEndPoint = new IPEndPoint(IpAddress, 2002);
        }
        
        public static void Start()
        {
            try
            {
                using var listener = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(LocalEndPoint);
                // make this listener synchronous deliberately
                listener.Listen(1);

                Console.WriteLine("Waiting for a connection.. (listener/" + LocalEndPoint + ')');
                using var handler = listener.Accept();
                Console.WriteLine("Connection established with: client/" + handler.RemoteEndPoint + '\n');
                
                while (true)
                {
                    var bytes = new byte[handler.ReceiveBufferSize];
                    var bytesReceived = handler.Receive(bytes);
                    var data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                    WriteReceived(data + $" (from client/{handler.RemoteEndPoint})");
                    var sentMessage = $"Thanks for the message! ({data})";
                    var sentMessageBytes = Encoding.ASCII.GetBytes(sentMessage);
                    handler.Send(sentMessageBytes);
                    WriteSent(sentMessage);
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Something went wrong while trying to start the socket listener...\n\t\tError content: " +
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