using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace BusVbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "BusVbot";

            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}