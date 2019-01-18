﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebChat.Data;
using WebChat.Models;
using WebChat.ViewModel;

namespace WebChat.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext dbContext;

        public HomeController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult PrivateChat()
        {
          return View(dbContext.Users.ToList());
        }

        [Authorize]
        public IActionResult Chat()
        {
            var chatViewModel = new ChatViewModel();
            chatViewModel.Users = this.dbContext.Users.ToList();
            chatViewModel.Messages= this.dbContext.Messages.Where(x => x.IsPrivate == false).ToList();
            return View(chatViewModel);
          
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
