using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task StartAsync()
        {
            try
            {
                using var listener = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(LocalEndPoint);
                // make this listener synchronous deliberately
                listener.Listen(1);

                Console.WriteLine("Waiting for a connection.. (listener/" + LocalEndPoint + ')');
                using var handler = await listener.AcceptAsync();
                Console.WriteLine("Connection established with: client/" + handler.RemoteEndPoint + '\n');
                var processesList = new List<Task>
                {
                    {Task.Run(async () => await SendMessageAsync(handler))},
                    {Task.Run(async () => await ReceiveMessageAsync(handler))}
                };
                Task.WaitAll(processesList.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Something went wrong while trying to start the socket listener...\n\t\tError content: " +
                    e.Message);
                throw;
            }
        }

        private static async Task ReceiveMessageAsync(Socket handler)
        {
            while (true)
            {
                var bytes = new byte[handler.ReceiveBufferSize];
                var bytesReceived = await handler.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                var data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                WriteReceived(data, handler.RemoteEndPoint);
                AskForNewMessage();
            }
        }

        private static async Task SendMessageAsync(Socket handler)
        {
            while (true)
            {
                var message = ReturnNewMessage();
                if (message.Length == 0)
                {
                    ShowErrorMessage("At least one character required to send a message!");
                    continue;
                }
                var trimmedMsg = message.Trim();
                var msgBytes = Encoding.ASCII.GetBytes(trimmedMsg);
                await handler.SendAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
                WriteSent(trimmedMsg, handler.RemoteEndPoint);
            }
        }

        private static void AskForNewMessage()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Write your message: ");
            Console.ResetColor();
        }
        
        private static string ReturnNewMessage()
        {
            AskForNewMessage();
            var message = Console.ReadLine() ?? "Default Message";
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
            ClearCurrentConsoleLine();
            return message;
        }
        
        private static void WriteSent(string content, EndPoint to)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"SENT (--> {to}): ");
            Console.ResetColor();
            Console.WriteLine(content);
        }

        private static void WriteReceived(string content, EndPoint from)
        {
            ClearCurrentConsoleLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"RECEIVED (<-- {from}): ");
            Console.ResetColor();
            Console.WriteLine(content);
        }

        private static void ClearCurrentConsoleLine()
        {
            var currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, currentLineCursor);
        }
        
        private static void ShowErrorMessage(string e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"ERROR: ");
            Console.ResetColor();
            Console.WriteLine(e);
        }
    }
}