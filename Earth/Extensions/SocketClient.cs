using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public static async Task SendMessageAsync()
        {
            try
            {
                using var sender = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Bind(LocalEndPoint);
                await sender.ConnectAsync(RemoteEndPoint);

                if (sender.Connected)
                {
                    Console.WriteLine($"Connected to Sun on {RemoteEndPoint} (Socket-Listener)..\n");
                }
                
                var processesList = new List<Task>
                {
                    {Task.Run(async () => await SendMessageAsync(sender))},
                    {Task.Run(async () => await ReceiveMessageAsync(sender))}
                };
                Task.WaitAll(processesList.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Something went wrong while trying to create sender socket / connect to remote listener\n\tError content: " +
                    e.Message);
                throw;
            }
        }

        private static async Task ReceiveMessageAsync(Socket sender)
        {
            while (true)
            {
                var bytes = new byte[sender.ReceiveBufferSize];
                var bytesReceived = await sender.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                var data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                ClearCurrentConsoleLine();
                WriteReceived(data);
                AskForNewMessage();
            }
        }
        
        private static async Task SendMessageAsync(Socket sender)
        {
            while (true)
            {
                AskForNewMessage();
                var message = Console.ReadLine() ?? "Default message";
                if (message.Length == 0) ShowErrorMessage("At least one character required to send a message!");
                var trimmedMsg = message.Trim();
                var msgBytes = Encoding.ASCII.GetBytes(trimmedMsg);
                await sender.SendAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
                WriteSent(trimmedMsg);
            }
        }
        
        private static void AskForNewMessage()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Write your message: ");
            Console.ResetColor();
        }

        private static void WriteSent(string content)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"SENT (--> {RemoteEndPoint}): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(content);
        }

        private static void WriteReceived(string content)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"RECEIVED (<-- {RemoteEndPoint}): ");
            Console.ForegroundColor = ConsoleColor.White;
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