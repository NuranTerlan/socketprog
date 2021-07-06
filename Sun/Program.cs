using System;
using System.Threading;
using System.Threading.Tasks;
using Sun.Extensions;

namespace Sun
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Listener (Sun)";
            await SocketListener.StartAsync();
        }
    }
}