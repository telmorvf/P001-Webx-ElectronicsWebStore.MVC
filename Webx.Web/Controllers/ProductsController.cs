using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStockRepository _stockRepository;
        private readonly DataContext _dataContext;
        private readonly IImageHelper _imageHelper;
        private readonly INotyfService _toastNotification;
        private readonly IConverterHelper _converterHelper;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IStoreRepository storeRepository,
            IStockRepository stockRepository,
            DataContext dataContext,
            IImageHelper imageHelper,
            INotyfService toastNotification,
            IConverterHelper converterHelper
            )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _storeRepository = storeRepository;
            _stockRepository = stockRepository;
            _dataContext = dataContext;
            _imageHelper = imageHelper;
            _toastNotification = toastNotification;
            _converterHelper = converterHelper;
        }

#nullable enable
        public async Task<IActionResult> Index(string? categoryString)
        {
            List<ProductsAllViewModel> productsList = new List<ProductsAllViewModel>();

            if (categoryString == null)
            {
                categoryString = "AllCategories";
            }

            var products = await _productRepository.GetFullProducts(categoryString);

            if (products == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            else
            {
                foreach (var product in products)
                {
                    var model = new ProductsAllViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Brand = product.Brand,
                        Category = product.Category,
                        IsService = product.IsService
                    };
                    productsList.Add(model);
                }

                ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
                return View(productsList);
            }
        }
#nullable disable

        public async Task<IActionResult> Details(int? id)
        {
            var product = await _productRepository.GetFullProduct(id.Value);

            if (product == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            //var model = _converterHelper.ToDetailProductViewModel(product);
            //return View(model);
            return View(product);
        }

        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Product> products;

            products = await _productRepository.GetProductAllAsync();

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Product);

            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new ProductViewModel();

            model.Categories = _productRepository.GetCategoriesCombo();
            model.Brands = _productRepository.GetBrandsCombo();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var product = _productRepository.GetProductByNameAsync(model.Name);
                if (product.Result != null)
                {
                    _toastNotification.Error("This Product Name Already Exists, Please try again...");
                    return View(model);
                }

                if (product.Result == null)
                {
                    try
                    {
                        // TODO: Pictures
                        Guid imageId = Guid.Empty;
                        if (model.PictureFile != null && model.PictureFile.Length > 0)
                        {

                            




                            // Filipe: Convert image bit array and upload to Azure
                            //imageId = await _imageHelper.UploadImageAsync(model.PictureFile, model.ImagesId, "products");
                            //model.ImageFirst = imageId;
                        }

                        // TODO: Telmo product validar no create se campos preenchidos (javascript)
                        Product newProduct = _converterHelper.ProductFromViewModel(model, true);

                        // Create the product
                        var minQuantity = model.MinimumQuantity;
                        await _productRepository.CreateAsync(newProduct);


                        // Stores & Stock - Start: Creat the association product to the store - Stocks Table                        
                        IEnumerable<Store> storeAll = await _storeRepository.GetAllStoresAsync();
                        foreach (var store in storeAll)
                        {
                            _dataContext.Stocks.Add(new Stock
                            {
                                Id = 0,
                                ProductId = newProduct.Id,
                                StoreId = store.Id,
                                Quantity = 0,
                                MinimumQuantity = minQuantity,
                            });
                        }
                        await _dataContext.SaveChangesAsync();
                        // Stores & Stock - End

                        _toastNotification.Success("Store created successfully!!!");
                        //converterHelper - refresh the create view after create
                        model = _converterHelper.ProductToViewModel(newProduct);

                        return View(model);
                    }
                    catch (Exception)
                    {
                        _toastNotification.Error("There was a problem, When try creating the product. Please try again");

                        model.Categories = _productRepository.GetCategoriesCombo();
                        model.Brands = _productRepository.GetBrandsCombo();
                        return View(model);
                    }
                }
            };
            return View(model);
        }





        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            var product = await _productRepository.GetProductByIdAsync(id.Value);
            var model = new ProductViewModel();
            if (product != null)
            {
                //converterHelper
                model = _converterHelper.ProductToViewModel(product);
            }
            else
            {
                _toastNotification.Error("Product could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(ProductViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var product = await _productRepository.GetProductByIdAsync(model.Id);
                if (product == null)
                {
                    _toastNotification.Error("Error, the brand was not found");
                    return View(model);
                };

                try
                {
                    //converterHelper
                    //var product = _converterHelper.ProductFromViewModel(model, false);

                    // TODO: Question Equipa: pq não concigo usar converterHelper acima, como descartar os nullos
                    product.Id = model.Id;
                    product.Name = model.Name;
                    product.Price = model.Price;
                    product.Description = model.Description;
                    product.IsService = model.IsService;
                    product.CategoryId = Convert.ToInt32(model.CategoryId);
                    product.BrandId = Convert.ToInt32(model.BrandId);

                    _dataContext.Products.Update(product);
                    await _dataContext.SaveChangesAsync();
                    
                    //converterHelper
                    model = _converterHelper.ProductToViewModel(product);

                    _toastNotification.Success("Brand changes saved successfully!!!");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        _toastNotification.Error($"The product  {model.Name}  already exists!");
                    }
                    else
                    {
                        _toastNotification.Error($"There was a problem updating the product, try again later!");
                    }

                    return View(model);
                }
            };
            return View(model);
        }





        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Product/ProductDetails")]
        public async Task<JsonResult> ProductDetails(int? Id)
        {
            if (Id == null)
            {
                _toastNotification.Error("Product Id was not found.");
                return null;
            }

            var product = await _productRepository.GetProductByIdAsync(Id.Value);
            ProductViewModel model = new ProductViewModel();

            if (product != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.ProductToViewModel(product);
            }
            else
            {
                return null;
            }

            if (model == null)
            {
                return null;
            }

            var json = Json(model);
            return json;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Product/ToastNotification")]
        public JsonResult ToastNotification(string message, string type)
        {
            bool result = false;

            if (type == "success")
            {
                _toastNotification.Success(message, 5);
                result = true;
            }

            if (type == "error")
            {
                _toastNotification.Error(message, 5);
                result = true;
            }

            if (type == "warning")
            {
                _toastNotification.Warning(message, 5);
                result = true;
            }

            if (type == "Information")
            {
                _toastNotification.Information(message, 5);
                result = true;
            }

            return Json(result);
        }
    }
}
