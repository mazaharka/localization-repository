using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LocalizationRep.Models;
using LocalizationRep.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using LocalizationRep.Utilities;

namespace LocalizationRep.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FileActionXML FAXML;
        private readonly LocalizationRepContext _context;
        private readonly IWebHostEnvironment _appEnvironment;

        public HomeController(ILogger<HomeController> logger, LocalizationRepContext context, IWebHostEnvironment appEnvironment)
        {
            FAXML = new FileActionXML(context, appEnvironment);
            _context = context;
            _appEnvironment = appEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        //private async void GetBigListAsync()
        //{
        //    var myTask = Task.Run(() => FAXML.ReadFileXMLAction());
        //    // your thread is free to do other useful stuff right nw

        //    // after a while you need the result, await for myTask:
        //    //var result =
        //        await myTask;

        //    // you can now use the results of loading:

        //    //return result;
        //}


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
