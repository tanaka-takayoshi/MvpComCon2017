using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatApp.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseUrl = "http://chat.52.175.232.56.nip.io/chat";

            Console.WriteLine("Connecting to {0}", baseUrl);
            HubConnection connection =  ConnectAsync(baseUrl).Result;
            Console.WriteLine("Connected to {0}", baseUrl);

            connection.SendAsync("SendFromConsole", "console@example.net", "test from Console").Wait();
            
            connection.On<string, string>("broadcastMessage", (name, msg) =>
            {
                Console.WriteLine(name + ":" + msg);
            });
            Console.ReadLine();
        }

        private static async Task<HubConnection> ConnectAsync(string baseUrl)
        {
            // Keep trying to until we can start
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
                    await Task.Delay(1000);
                }
            }
        }
    }
}
