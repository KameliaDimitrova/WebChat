﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Serialization;
using WebChat.Data;
using WebChat.Models;
using WebChat.Models.Chat;

namespace WebChat.Hubs
{

    [Authorize]
    public class ChatHub : Hub
    {
        private ApplicationDbContext dbContext;

        public ChatHub(ApplicationDbContext db)
        {
            this.dbContext = db;
        }
        static HashSet<string> CurrentConnections = new HashSet<string>();

        public async Task OnConnected()
        {
            var id = Context.ConnectionId;
            CurrentConnections.Add(id);

            await base.OnConnectedAsync();
        }



        public List<string> GetAllActiveConnections()
        {
            return CurrentConnections.ToList();
        }


        static List<User> SignalRUsers = new List<User>();

        public void Connect(string userId)
        {
            var userName = this.dbContext.Users.FirstOrDefault(x => x.Id == userId).UserName;
            var id = Context.ConnectionId;

            if (SignalRUsers.Count(x => x.ConnectionId == id) == 0)
            {
                SignalRUsers.Add(new User { ConnectionId = id, UserName = userName });
            }
        }

        public async Task Send(string message)
        {
            var sendMessage = new Message
            {
                ConnectionId = this.Context.ConnectionId,
                FromUserName = this.Context.User.Identity.Name,
                IsPrivate = false,
                Text = message,
                FromUserID = this.dbContext.Users.FirstOrDefault(x=>x.UserName== this.Context.User.Identity.Name).Id
            };
            this.dbContext.Messages.Add(sendMessage);
            this.dbContext.SaveChanges();
            await this.Clients.All.SendAsync("NewMessage", sendMessage);
        }

        public async Task SendPrivateMessage(string message, string reciverId)
        {

            var senderId = this.dbContext.Users.FirstOrDefault(x => x.UserName == this.Context.User.Identity.Name).Id;
            var sendMessage = new Message
            {
                ConnectionId = senderId+reciverId,
                FromUserName = this.Context.User.Identity.Name,
                ToUserName = this.dbContext.Users.FirstOrDefault(x => x.Id == reciverId).UserName,
                FromUserID = senderId,
                IsPrivate = true,
                ToUserID = reciverId,
                Text = message,
            };
            this.dbContext.Messages.Add(sendMessage);
            this.dbContext.SaveChanges();
            await this.Clients.Users(reciverId, senderId).SendAsync("NewMessage", sendMessage);
        }
    }
}
