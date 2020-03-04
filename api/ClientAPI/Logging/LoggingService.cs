using System;
using System.Collections.Generic;
using System.Text;

using Serilog;
using Serilog.Exceptions;
using Microsoft.Extensions.Configuration;

namespace ClientAPI.Logging
{
    public static class LoggingService
    {
        public static ILogger BuildLogger(IConfigurationRoot config) =>
            new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile(
                    (config.GetValue<string>("Logging:SerilogSettings:OutputFilename") ?? "logging") + "_{Date}.log")
                .Enrich.WithExceptionDetails()
                .CreateLogger();
    }
}
