using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Helpers;
using Webx.Web.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Webx.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlobHelper _blobHelper;

        public HomeController(
            ILogger<HomeController> logger,
            IBlobHelper blobHelper)
        {
            _logger = logger;
            _blobHelper = blobHelper;
        }

        public IActionResult Index()
        {
            return View("CommingSoon", "Home");
        }

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
