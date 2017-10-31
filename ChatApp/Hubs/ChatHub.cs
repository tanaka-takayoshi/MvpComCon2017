using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(string message)
        {
            var name2 = Context.User.FindAll(c =>
            {
                Console.WriteLine(c.Type + "=" + c.Value);
                return c.Type == "emails";
                }).FirstOrDefault()?.Value ?? "anonymous@example.net";
            // Call the broadcastMessage method to update clients.
            Clients.All.InvokeAsync("broadcastMessage", name2, message);
        }

        public void SendFromConsole(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.InvokeAsync("broadcastMessage", name, message);
        }
    }
}
