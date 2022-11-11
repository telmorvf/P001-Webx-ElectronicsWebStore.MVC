using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Controllers
{
   
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository; 
        private readonly INotyfService _toastNotification;

        public CartController(IProductRepository productRepository, INotyfService toastNotification)
        {
            _productRepository = productRepository;        
            _toastNotification = toastNotification;
        }

      
        public async Task<IActionResult> Index()
        {
            var model = await _productRepository.GetInitialShopViewModelAsync();
            return View(model);
        }

       

       
    }
}
