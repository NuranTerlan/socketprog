using System;
using Earth.Extensions;

namespace Earth
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Client (Earth)";
            SocketClient.SendContinuousMessages();
        }
    }
}