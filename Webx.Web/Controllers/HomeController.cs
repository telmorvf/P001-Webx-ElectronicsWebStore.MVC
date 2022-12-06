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
using Webx.Web.Data.Entities;
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
        private readonly IBrandRepository _brandRepositoty;
        private readonly IStockRepository _stockRepository;
        private readonly IConverterHelper _converterHelper;

        public HomeController(
            ILogger<HomeController> logger,
            IBlobHelper blobHelper, IUserHelper userHelper,
            ICategoryRepository categoryRepository,
            INotyfService toastNotification,
            IProductRepository productRepository,
            IBrandRepository brandRepositoty,
            IStockRepository stockRepository,
            IConverterHelper converterHelper)
        {
            _logger = logger;
            _blobHelper = blobHelper;
            _userHelper = userHelper;
            _categoryRepository = categoryRepository;            
            _toastNotification = toastNotification;
            _productRepository = productRepository;
            _brandRepositoty = brandRepositoty;
            _stockRepository = stockRepository;
            _converterHelper = converterHelper;
        }

        public async Task<IActionResult> Index()
        {

            //ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();

            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();


            if (model == null)
            {
                return NotFound();
            }

            List<Product> sugestedProducts = new List<Product>();
            sugestedProducts.Add(await _productRepository.GetProductByNameAsync("Motherboard ATX Asus Prime B550-Plus"));
            sugestedProducts.Add(await _productRepository.GetProductByNameAsync("RAM Memory Corsair Vengeance RGB 32GB (2x16GB) DDR5-6000MHz CL36 White"));

            model.SuggestedProducts = sugestedProducts;
            model.Product = await _productRepository.GetProductByNameAsync("Intel Core i9-11900K 8-Core 3.5GHz W/Turbo 5.3GHz 16MB Skt1200 Processor");
            var products = await _productRepository.GetHighlightedProductsAsync();
            model.HighlightedProducts = await _converterHelper.ToProductsWithReviewsViewModelList(products);
            model.Stocks = await _stockRepository.GetAllStockWithStoresAsync();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                ViewBag.UserFullName = user.FullName;
                ViewBag.IsActive = user.Active;
            }

            var cookiesConsent = _productRepository.CheckCookieConsentStatus();
            model.CookieConsent = cookiesConsent;
            model.Brands = (List<Brand>)await _brandRepositoty.GetAllBrandsAsync();

            return View(model);
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
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
            model.Brands = (List<Brand>)await _brandRepositoty.GetAllBrandsAsync();
            return View(model);
        }
    }
}
