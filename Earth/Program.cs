using System;
using System.Threading;
using System.Threading.Tasks;
using Earth.Extensions;

namespace Earth
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Client (Earth)";
            await Task.Delay(100);
            Console.Write("Port of instance: ");
            var port = int.Parse(Console.ReadLine() ?? "1918");
            var client = new SocketClient(port);
            await client.SendMessageAsync();
        }
    }
}