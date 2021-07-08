using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        private static readonly Dictionary<string, Socket> Connections;

        static SocketListener()
        {
            var host = Dns.GetHostEntry("localhost");
            IpAddress = host.AddressList[0];
            LocalEndPoint = new IPEndPoint(IpAddress, 2002);
            Connections = new Dictionary<string, Socket>();
        }

        public static async Task StartAsync()
        {
            try
            {
                using var listener = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(LocalEndPoint);
                // make this listener synchronous deliberately
                listener.Listen(20);

                Console.WriteLine("Waiting for a connection.. (listener/" + LocalEndPoint + ")\n");
                var counter = 1;
                while (true)
                {
                    var handler = await listener.AcceptAsync();
                    Connections.Add("connection" + counter++, handler);
                    ClearCurrentConsoleLine();
                    Console.WriteLine("Connection established with: client/" + handler.RemoteEndPoint);
                    Task.Run(async () => await ReceiveMessageAsync(handler));
                    Task.Run(async () => await SendMessageAsync());

                    // Console.Write("Connect to => (");
                    // Console.Write(string.Join(",", Connections.Keys));
                    // Console.Write(")(type exact same): ");
                    // var connectionName = Console.ReadLine();
                    // await HandleConnection(Connections[connectionName ?? string.Empty]);
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

        private static async Task ReceiveMessageAsync(Socket handler)
        {
            while (true)
            {
                var bytes = new byte[handler.ReceiveBufferSize];
                var bytesReceived = await handler.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                var data = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                WriteReceived(data, handler.RemoteEndPoint);
                if (Connections.ContainsKey(data.Split()[^1]))
                {
                    await SendDirectMessageAsync(data);
                }

                if (data.Split()[^1].Equals("@all"))
                {
                    var message = GenerateMessageWithBlocks(data.Split());
                    var currentConnectionName =
                        Connections.FirstOrDefault(s => s.Value.RemoteEndPoint == handler.RemoteEndPoint).Key;
                    await SendMessageToEveryOneExceptAsync(message, currentConnectionName);
                }

                AskForNewMessage();
            }
        }

        private static async Task SendMessageAsync(Socket handler)
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
                await handler.SendAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
                WriteSent(message, handler.RemoteEndPoint);
            }
        }

        private static async Task SendMessageAsync()
        {
            while (true)
            {
                var text = ReturnNewMessage().Trim();
                if (text.Length == 0)
                {
                    ShowErrorMessage("At least one character required to send a message!");
                    continue;
                }

                if (Connections.ContainsKey(text.Split()[^1]))
                {
                    await SendDirectMessageAsync(text);
                    continue;
                }

                await SendMessageToEveryOneAsync(text);
            }
        }

        private static async Task SendMessageToEveryOneAsync(string message)
        {
            var msgBytes = Encoding.ASCII.GetBytes(message);
            foreach (var handler in Connections.Values)
            {
                await handler.SendAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
                WriteSent(message, handler.RemoteEndPoint);
            }
        }

        private static async Task SendMessageToEveryOneExceptAsync(string message, string exceptConnection)
        {
            var msgBytes = Encoding.ASCII.GetBytes(message);
            foreach (var handlerName in Connections.Keys.Where(n => !n.Equals(exceptConnection)))
            {
                var handler = Connections[handlerName];
                await handler.SendAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
                WriteSent(message, handler.RemoteEndPoint);
            }
        }

        private static async Task SendDirectMessageAsync(string text)
        {
            var (handler, message) = GetHandlerMessageFromText(text);
            var msgBytes = Encoding.ASCII.GetBytes(message);
            await handler.SendAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
            WriteSent(message, handler.RemoteEndPoint);
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

        private static Tuple<Socket, string> GetHandlerMessageFromText(string text)
        {
            var textBlocks = text.Split();
            var handler = Connections[textBlocks[^1]];
            var message = GenerateMessageWithBlocks(textBlocks);
            return Tuple.Create(handler, message);
        }

        private static string GenerateMessageWithBlocks(IReadOnlyCollection<string> blocks)
        {
            return string.Join(' ', blocks.Take(blocks.Count - 1));
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