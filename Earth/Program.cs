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
            await Task.Delay(300);
            await SocketClient.SendMessageAsync();
        }
    }
}