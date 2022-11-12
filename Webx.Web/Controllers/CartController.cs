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
        private readonly IUserHelper _userHelper;

        public CartController(IProductRepository productRepository, INotyfService toastNotification,IUserHelper userHelper)
        {
            _productRepository = productRepository;        
            _toastNotification = toastNotification;
            _userHelper = userHelper;
        }

      
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                ViewBag.UserFullName = user.FullName;
                ViewBag.IsActive = user.Active;
            }

            var model = await _productRepository.GetInitialShopViewModelAsync();
            return View(model);
        }


        public async Task<IActionResult> RemoveProduct(int? id)
        {
            var product = await _productRepository.GetFullProduct(id.Value);

            if (product == null)
            {
                return NotFound();
            }
            var cart = await _productRepository.GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound();
            }

            foreach (var item in cart)
            {
                if (product.Id == item.Product.Id)
                {
                    cart.Remove(item);
                    break;
                }
            }

            var response = _productRepository.UpdateCartCookie(cart);
            if (response.IsSuccess == true)
            {
                var model = new ShopViewModel { Cart = cart };

                return PartialView("_CartPartialView", model);
            }
            else
            {
                _toastNotification.Error($"There was an error updating the cart. {response.Message}");
                return NotFound();
            }

        }

        [HttpGet]
        public async Task<IActionResult> UpdateCart(int? id, string quantity)
        {
            var product = await _productRepository.GetFullProduct(id.Value);

            if (product == null)
            {
                return NotFound();
            }
            var cart = await _productRepository.GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound();
            }

            int value;
            if (int.TryParse(quantity, out value))
            {
                bool productInCart = false;

                foreach (var item in cart)
                {
                    if (product.Id == item.Product.Id)
                    {
                        if (value == 1)
                        {
                            item.Quantity++;
                        }
                        else
                        {
                            item.Quantity--;
                            if (item.Quantity == 0)
                            {
                                cart.Remove(item);
                            }
                        }

                        productInCart = true;
                        break;
                    }
                }

                if (!productInCart && value == 1)
                {
                    cart.Add(new CartViewModel { Product = product, Quantity = 1 });
                }

                var response = _productRepository.UpdateCartCookie(cart);
                if(response.IsSuccess == true)
                {
                    var model = new ShopViewModel { Cart = cart };

                    return PartialView("_CartPartialView", model);
                }
                else
                {
                    _toastNotification.Error($"There was an error updating the cart. {response.Message}");
                    return NotFound();
                }
                
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        public async Task<IActionResult> RemoveProductFromDrowpDown(int? id,string quantity)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetByIdAsync(id.Value);

            if (product == null)
            {
                return NotFound();
            }

            var cart = await _productRepository.GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound();
            }
  

            foreach (var item in cart)
            {
                if (product.Id == item.Product.Id)
                {
                    if(quantity == "-1")
                    {
                        item.Quantity--;
                        if (item.Quantity == 0)
                        {
                            cart.Remove(item);
                        }
                    }
                    else
                    {
                        cart.Remove(item);
                    }

                    break;
                }
            }

            var response = _productRepository.UpdateCartCookie(cart);
            if (response.IsSuccess == true)
            {
                var model = new ShopViewModel { Cart = cart };

                return PartialView("_CartDropDownPartial", model);
            }
            else
            {
                _toastNotification.Error($"There was an error updating the cart. {response.Message}");
                return NotFound();
            }

        }

        [HttpGet]
        public IActionResult ClearCart()
        {
            var response = _productRepository.ClearCart();

            if (response.IsSuccess)
            {
                var model = new ShopViewModel { Cart = new List<CartViewModel>() };                
                return PartialView("_CartPartialView", model);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult UpdateDrowpDown() 
        {
            var model = new ShopViewModel { Cart = new List<CartViewModel>()};
            return PartialView("_CartDropDownPartial", model);
        }


        [HttpPost]
        public async Task<JsonResult> GetProductDetails(int id)
        {
            var product = await _productRepository.GetFullProduct(id);
            return Json(product);
        }


    }
}
