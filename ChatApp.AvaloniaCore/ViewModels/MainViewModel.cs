using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Sockets.Client;
using Microsoft.AspNetCore.Sockets;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Avalonia.Threading;


namespace ChatApp.AvaloniaCore.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public ObservableCollection<MessageItem> Items { get; }
        public ReactiveCommand SendCommand { get; }

        private string sendMessage = "";
        public string SendMessage
        {
            get { return sendMessage; }
            set { this.RaiseAndSetIfChanged(ref sendMessage, value); }
        }

        private HubConnection connection;

        public MainViewModel()
        {

            Items = new ObservableCollection<MessageItem>();
            
            
            SendCommand =   ReactiveCommand.Create<object>(param =>
            {
                var msg = SendMessage;
                Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await Executor.Connection.SendAsync("SendFromConsole", "fedora@example.net", msg);
            
                        SendMessage = "";
                    });
            });

            Executor.Connection.On<string, string>("broadcastMessage", (name, msg) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Items.Add(new MessageItem
                        {
                            Message = msg,
                            Email = name
                        });
                    });
                // Observable.Start(() =>
                // {
                //     Console.WriteLine("Received");
                // }).ObserveOn(AvaloniaScheduler.Instance)
                // .Subscribe(_ => 
                // {
                //     Console.WriteLine("UI Updating");
                //     Items.Add(new MessageItem
                //     {
                //         Message = msg,
                //         Email = name
                //     });
                // });
            });
        }

        private async Task<int> StartAsync()
        {
            var baseUrl = "http://chat.52.175.232.56.nip.io/chat";

            Console.WriteLine("Connecting to {0}", baseUrl);
            connection = await ConnectAsync(baseUrl);
            Console.WriteLine("Connected to {0}", baseUrl);

            connection.On<string, string>("broadcastMessage", (name, msg) =>
            {
                Items.Add(new MessageItem
                {
                    Message = msg,
                    Email = name
                });
            });
            return 0;
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

                var connection = new HubConnectionBuilder()
                                .WithMessageHandler(handler)
                                .WithTransport(TransportType.WebSockets)
                                .WithUrl(baseUrl)
                                .WithConsoleLogger(LogLevel.Trace)
                                .Build();

                try
                {
                    Console.WriteLine("Staring");
                    await connection.StartAsync();
                    Console.WriteLine("Stared");
                    return connection;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await Task.Delay(1000);
                }
            }
        }
    }

    public class MessageItem
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }
}