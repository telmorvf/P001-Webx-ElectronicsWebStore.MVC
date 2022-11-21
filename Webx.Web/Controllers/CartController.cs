using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        private readonly IStockRepository _stockRepository;
        private readonly IStoreRepository _storeRepository;

        public CartController(IProductRepository productRepository,
            INotyfService toastNotification,
            IUserHelper userHelper,
            IStockRepository stockRepository,
            IStoreRepository storeRepository)
        {
            _productRepository = productRepository;        
            _toastNotification = toastNotification;
            _userHelper = userHelper;
            _stockRepository = stockRepository;
            _storeRepository = storeRepository;
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
            model.Stocks = await _stockRepository.GetAllStockWithStoresAsync();
            model.Stores = _storeRepository.GetComboStores();

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
                var model = new ShopViewModel { 
                    Cart = cart,
                    Stores = _storeRepository.GetComboStores()
                };

                return PartialView("_CartPartialView", model);
            }
            else
            {
                _toastNotification.Error($"There was an error updating the cart. {response.Message}");
                return NotFound();
            }

        }

        [HttpGet]
        public async Task<IActionResult> ChangeStore(int id, int storeId)
        {
            var product = await _productRepository.GetFullProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            var cart = await _productRepository.GetCurrentCartAsync();

            if (cart == null)
            {
                return NotFound();
            }

            var store = await _storeRepository.GetByIdAsync(storeId);

            if(store == null)
            {
                return NotFound();
            }

          
            foreach (var item in cart)
            {
                if (item.Product.Id == product.Id)
                {
                    item.StoreId = storeId;
                    if (product.IsService)
                    {
                        item.Color = "Green";
                    }
                    else
                    {
                        var color = await _stockRepository.GetProductStockColorFromStoreIdAsync(product.Id, storeId);
                        item.Color = color;
                    }                    
                }
            }

            var response = _productRepository.UpdateCartCookie(cart);
            if (response.IsSuccess == true)
            {
                var model = new ShopViewModel
                {
                    Cart = cart,
                    Stores = _storeRepository.GetComboStores()
                };

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
                    cart.Add(new CartViewModel {
                        Product = product,
                        Quantity = 1,
                        StoreId = await _storeRepository.GetOnlineStoreIdAsync(),
                        //Color = await _stockRepository.GetProductStockColorFromStoreIdAsync(product.Id,StoreId)
                    });
                }

                var response = _productRepository.UpdateCartCookie(cart);
                if(response.IsSuccess == true)
                {
                    var model = new ShopViewModel {
                        Cart = cart,
                        Stores = _storeRepository.GetComboStores()
                    };

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
                var model = new ShopViewModel {
                    Cart = new List<CartViewModel>(),
                    Stores = _storeRepository.GetComboStores()
                };                
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

        [HttpPost]
        public async Task<JsonResult> CheckStock(int id,int quantity,int desiredQuantity)
        {
            if(quantity == 1)
            {
                var product = await _productRepository.GetFullProduct(id);
                var cart = await _productRepository.GetCurrentCartAsync();
                bool isInStock = false;
                var storeName = "";

                foreach (var item in cart)
                {
                    if (item.Product.Id == product.Id)
                    {
                        if (product.IsService)
                        {
                            isInStock = true;
                        }
                        else
                        {
                            var stock = await _stockRepository.GetProductStockInStoreAsync(product.Id, item.StoreId);
                            var store = await _storeRepository.GetAllStoreByIdAsync(item.StoreId);
                            storeName = store.Name;
                            if ((stock.Quantity - (desiredQuantity + 1)) > 0)
                            {
                                isInStock = true;
                            }
                            else
                            {
                                isInStock = false;
                            }
                        }                        
                    }
                }

                var itemToJson = new
                {
                    stock = isInStock,
                    product = product.Name,
                    store = storeName
                };

                return Json(itemToJson);
            }
            else
            {
                var itemToJson = new
                {
                    stock = true,                 
                };

                return Json(itemToJson);
            }           

            
        }


    }
}
