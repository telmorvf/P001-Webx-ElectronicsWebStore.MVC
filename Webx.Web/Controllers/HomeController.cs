using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data;
using Webx.Web.Helpers;
using Webx.Web.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Webx.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly INotyfService _toastNotification;

        public HomeController(
            ILogger<HomeController> logger,
            IBlobHelper blobHelper, IUserHelper userHelper,
            DataContext context, INotyfService toastNotification)
        {
            _logger = logger;
            _blobHelper = blobHelper;
            _userHelper = userHelper;
            _context = context;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                ViewBag.UserFullName = user.FullName;
            }
            
            return View(/*"CommingSoon", "Home"*/);
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


        [Route("error/404")]
        public async Task<IActionResult> Error404()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }
    }
}
