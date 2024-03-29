﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebCompiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
             //.ConfigureServices(x => x.AddAutofac())
                .UseStartup<Startup>();
    }
}
