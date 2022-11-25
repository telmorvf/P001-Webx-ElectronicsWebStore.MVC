using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly ICategoryRepository _categoryRepository;        
        private readonly INotyfService _toastNotification;
        private readonly IProductRepository _productRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IBlobHelper blobHelper, IUserHelper userHelper,
            ICategoryRepository categoryRepository, INotyfService toastNotification,IProductRepository productRepository)
        {
            _logger = logger;
            _blobHelper = blobHelper;
            _userHelper = userHelper;
            _categoryRepository = categoryRepository;            
            _toastNotification = toastNotification;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {

            //ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();

            var model = await _productRepository.GetInitialShopViewModelAsync();            

            if (model == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                ViewBag.UserFullName = user.FullName;
                ViewBag.IsActive = user.Active;
            }

            var cookiesConsent = _productRepository.CheckCookieConsentStatus();
            model.CookieConsent = cookiesConsent;

            return View(model);
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
            var model = await _productRepository.GetInitialShopViewModelAsync();
            return View(model);
        }
    }
}
