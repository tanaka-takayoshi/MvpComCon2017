using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Logging.Serilog;
using Avalonia.Themes.Default;
using Avalonia.Markup.Xaml;
using Serilog;

using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Sockets.Client;
using Microsoft.AspNetCore.Sockets;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ChatApp.AvaloniaCore
{
    class App : Application
    {

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var config = builder.Build();
            InitializeLogging();
            Executor.Main2(config["chatServerUrl"]);
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .Start<MainWindow>();
        }

        public static void AttachDevTools(Window window)
        {
#if DEBUG
            DevTools.Attach(window);
#endif
        }

        private static void InitializeLogging()
        {
#if DEBUG
            // SerilogLogger.Initialize(new LoggerConfiguration()
            //     .MinimumLevel.Warning()
            //     .WriteTo.Trace(outputTemplate: "{Area}: {Message}")
            //     .CreateLogger());
#endif
        }
    }
    static class Executor
    {
        public static HubConnection Connection;
        public static void Main2(string baseUrl)
        {
            Console.WriteLine("Connecting to {0}", baseUrl);
            Connection =  ConnectAsync(baseUrl).Result;
            Console.WriteLine("Connected to {0}", baseUrl);
        }

        private static async Task<HubConnection> ConnectAsync(string baseUrl)
        {
            // Keep trying to until we can start
            var transport = TransportType.WebSockets;
            while (true)
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                //handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                //{
                //    return true;
                //};
                var connection = new HubConnectionBuilder()
                                .WithMessageHandler(handler)
                                .WithTransport(transport)
                                .WithUrl(baseUrl)
                                .WithConsoleLogger(LogLevel.Trace)
                                .Build();
                try
                {
                    await connection.StartAsync();
                    return connection;
                }
                catch (Exception)
                {
                    //fallback
                    transport = TransportType.ServerSentEvents;
                    await Task.Delay(1000);
                }
            }
        }
    }
}
