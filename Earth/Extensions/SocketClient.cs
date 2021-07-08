using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Earth.Extensions
{
    public class SocketClient
    {
        private readonly IPAddress _ipAddress;
        private readonly IPEndPoint _remoteEndPoint;
        private readonly IPEndPoint _localEndPoint;

        public SocketClient(int port)
        {
            var host = Dns.GetHostEntry("localhost");
            _ipAddress = host.AddressList[0];
            _remoteEndPoint = new IPEndPoint(_ipAddress, 2002);
            _localEndPoint = new IPEndPoint(_ipAddress, port);
        }

        public async Task SendMessageAsync()
        {
            try
            {
                using var sender = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                sender.Bind(_localEndPoint);
                await sender.ConnectAsync(_remoteEndPoint);

                if (sender.Connected)
                {
                    Console.WriteLine($"Connected to Sun on {_remoteEndPoint} (Socket-Listener)..\n");
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

        private async Task ReceiveMessageAsync(Socket sender)
        {
            while (true)
            {
                var bytes = new byte[sender.ReceiveBufferSize];
                var bytesReceived = await sender.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                var data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                WriteReceived(data);
                AskForNewMessage();
            }
        }
        
        private async Task SendMessageAsync(Socket sender)
        {
            while (true)
            {
                var message = ReturnNewMessage().Trim();
                if (message.Length == 0)
                {
                    ShowErrorMessage("At least one character required to send a message!");
                    continue;
                }
                var msgBytes = Encoding.ASCII.GetBytes(message);
                await sender.SendAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
                WriteSent(message);
            }
        }
        
        private void AskForNewMessage()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Write your message: ");
            Console.ResetColor();
        }
        
        private string ReturnNewMessage()
        {
            AskForNewMessage();
            var message = Console.ReadLine() ?? "Default Message";
            Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
            ClearCurrentConsoleLine();
            return message;
        }

        private void WriteSent(string content)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"SENT (--> {_remoteEndPoint}): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(content);
        }

        private void WriteReceived(string content)
        {
            ClearCurrentConsoleLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"RECEIVED (<-- {_remoteEndPoint}): ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(content);
        }
        
        private void ClearCurrentConsoleLine()
        {
            var currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private void ShowErrorMessage(string e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"ERROR: ");
            Console.ResetColor();
            Console.WriteLine(e);
        }
    }
}