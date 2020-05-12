using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebSocketBridge.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(Program));
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var informationalVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            logger.LogInformation("Started {Program} v{Version} ({Information})", assemblyName.Name, assemblyName.Version, informationalVersion);

            host.Run();
        }

        // EF Core uses this method at design time to access the DbContext
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    if (args.Contains("--debug"))
                        logging.SetMinimumLevel(LogLevel.Debug);
                    if (args.Contains("--trace"))
                        logging.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
