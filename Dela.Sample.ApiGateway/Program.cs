﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dela.Sample.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:8087")
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", false, true);
                    builder.AddJsonFile("configuration.json", false, true);
                })
                .UseStartup<Startup>();
    }
}
