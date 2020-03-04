using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

using ClientAPI.Logging;
namespace ClientAPI
{
    public class Program
    {
        private static string _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(hostBuilder => {
                
                var config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build();

                    hostBuilder.UseKestrel(options =>
                    {
                        if (!string.IsNullOrEmpty(_environment) && _environment.ToLower().Equals("development"))
                        {
                            options.Listen(IPAddress.Loopback, 8080);
                        }
                        else
                        {
                            options.Listen(IPAddress.Any, 8080);
                        }
                    });

                    hostBuilder.UseSerilog(LoggingService.BuildLogger(config));
                    hostBuilder.UseStartup<Startup>();
            });
    }
}
