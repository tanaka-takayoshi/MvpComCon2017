﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(string message)
        {
            var name2 = Context.User.FindAll("email").FirstOrDefault()?.Value;
            // Call the broadcastMessage method to update clients.
            Clients.All.InvokeAsync("broadcastMessage", name2, message);
        }
    }
}
