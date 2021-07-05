using System;
using System.Threading;
using Sun.Extensions;

namespace Sun
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Listener (Sun)";
            Thread.Sleep(300);
            SocketListener.Start();
        }
    }
}