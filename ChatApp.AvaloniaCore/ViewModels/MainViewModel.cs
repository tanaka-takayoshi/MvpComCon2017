using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Sockets.Client;

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
            
            //var t = StartAsync().Result;
            
            SendCommand =   ReactiveCommand.Create<object>(param =>
            {
                var msg = SendMessage;
                // Items.Add(new MessageItem
                // {
                //     Message = msg,
                //     Email = "test"
                // });
                //connection.InvokeAsync<object>("boradcastMessage", "client@example.com", msg);
                SendMessage = "";
            });

        }

        private async Task<int> StartAsync()
        {
            var baseUrl = "https://chat.52.175.232.56.nip.io/";

            Console.WriteLine("Connecting to {0}", baseUrl);
            connection = await ConnectAsync(baseUrl);
            Console.WriteLine("Connected to {0}", baseUrl);

            // try
            // {

            //     var cts = new CancellationTokenSource();
                
            //     //cts.Cancel();

            //     // Set up handler
            //     //connection.On<string>("Send", Console.WriteLine);

            //     connection.Closed += e =>
            //     {
            //         Console.WriteLine("Connection closed.");
            //         cts.Cancel();
            //         return Task.CompletedTask;
            //     };

            //     var ctsTask = Task.Delay(-1, cts.Token);
            //     // await connection.InvokeAsync<object>("Send", line, cts.Token);
            // }
            // catch (AggregateException aex) when (aex.InnerExceptions.All(e => e is OperationCanceledException))
            // {
            // }
            // catch (OperationCanceledException)
            // {
            // }
            // finally
            // {
            //     //await connection.DisposeAsync();
            // }
            return 0;
        }

        private async Task<HubConnection> ConnectAsync(string baseUrl)
        {
            var connection = new HubConnectionBuilder()
                                .WithUrl(baseUrl)
                                //.WithConsoleLogger(LogLevel.Trace)
                                .Build();
            await connection.StartAsync();
            return connection;
        }
    }

    public class MessageItem
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }
}