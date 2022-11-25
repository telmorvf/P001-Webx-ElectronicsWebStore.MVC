using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
//using Syncfusion.EJ2.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IBlobHelper _blobHelper;
        private readonly IConverterHelper _converterHelper;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IStoreRepository storeRepository,
            IStockRepository stockRepository,
            DataContext dataContext,
            IImageHelper imageHelper,
            INotyfService toastNotification,
            IBlobHelper blobHelper,
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
            _blobHelper = blobHelper;
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


        public async Task<IActionResult> ViewAll(bool isService)
        {
            IEnumerable<Product> products;

            if (isService == true)
            {
                products = await _productRepository.GetServiceAllAsync();
                ViewBag.IsService = true;
            }
            else
            {
                products = await _productRepository.GetProductAllAsync();
                ViewBag.IsService = false;
            }

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Product);

            var stores = _dataContext.Stores.ToListAsync();
            ViewBag.FilterStore = stores;

            return View(products);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new ProductAddViewModel();

            model.Categories = _productRepository.GetCategoriesCombo();
            model.Brands = _productRepository.GetBrandsCombo();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductAddViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.Categories = _productRepository.GetCategoriesCombo();
                model.Brands = _productRepository.GetBrandsCombo();
                return View(model);
            }
            else
            {
                var product = _productRepository.GetProductByNameAsync(model.Name);
                if (product.Result != null)
                {
                    _toastNotification.Error("This Product Name Already Exists, Please try again...");
                    model.Categories = _productRepository.GetCategoriesCombo();
                    model.Brands = _productRepository.GetBrandsCombo();
                    return View(model);
                }

                if (product.Result == null)
                {
                    try
                    {
                        // CREATE
                        List<ProductImages> productImages = new List<ProductImages>();
                        Guid imageId = Guid.Empty;

                        if (model.UploadFiles != null && model.UploadFiles.Count > 0)
                        {
                            foreach (var file in model.UploadFiles)
                            {
                                if (file.Length > 0)
                                {
                                    using var image = Image.Load(file.OpenReadStream());
                                    image.Mutate(img => img.Resize(512, 0));

                                    using (MemoryStream m = new MemoryStream())
                                    {
                                        image.SaveAsJpeg(m);
                                        byte[] imageBytes = m.ToArray();
                                        imageId = await _blobHelper.UploadBlobAsync(imageBytes, "products");
                                    }

                                    productImages.Add(new ProductImages
                                    {
                                        ImageId = imageId
                                    });
                                }
                            }
                        }

                        //TODO Passar a Lista de Imagens
                        Product newProduct = new Product
                        {
                            Id = 0,
                            Name = model.Name,
                            Price = model.Price,
                            Description = model.Description,
                            IsService = model.IsService,
                            IsPromotion = model.IsPromotion,
                            CategoryId = Convert.ToInt32(model.CategoryId),
                            BrandId = Convert.ToInt32(model.BrandId),
                            Images = productImages
                        };
                        await _productRepository.CreateAsync(newProduct);

                        // Stores & Stock - Start: Creat the association product to the store - Stocks Table                        
                        var minQuantity = model.MinimumQuantity;
                        var recQuantity = model.ReceivedQuantity;

                        IEnumerable<Store> storeAll = await _storeRepository.GetAllStoresAsync();
                        foreach (var store in storeAll)
                        {
                            _dataContext.Stocks.Add(new Stock
                            {
                                Id = 0,
                                ProductId = newProduct.Id,
                                StoreId = store.Id,
                                Quantity = recQuantity,
                                MinimumQuantity = minQuantity,
                            });
                        }
                        await _dataContext.SaveChangesAsync();
                        // Stores & Stock - End

                        _toastNotification.Success("Product created successfully!!!");
                        //converterHelper - refresh the create view after create
                        model = _converterHelper.ProductAddToViewModel(newProduct);

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
        public IActionResult CreateService()
        {
            var model = new ServiceViewModel();

            model.Categories = _productRepository.GetCategoriesCombo();
            model.Brands = _productRepository.GetBrandsCombo();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(ServiceViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.Categories = _productRepository.GetCategoriesCombo();
                model.Brands = _productRepository.GetBrandsCombo();
                return View(model);
            }
            else
            {
                var product = _productRepository.GetProductByNameAsync(model.Name);
                if (product.Result != null)
                {
                    _toastNotification.Error("This Service Name Already Exists, Please try again...");
                    model.Categories = _productRepository.GetCategoriesCombo();
                    model.Brands = _productRepository.GetBrandsCombo();
                    return View(model);
                }

                if (product.Result == null)
                {
                    try
                    {
                        Product newService = _converterHelper.ServiceFromViewModel(model, true);

                        await _productRepository.CreateAsync(newService);

                        _toastNotification.Success("Service created successfully!!!");
                        //converterHelper - refresh the create view after create
                        model = _converterHelper.ServiceToViewModel(newService);

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
            if (!this.ModelState.IsValid)
            {
                model.Categories = _productRepository.GetCategoriesCombo();
                model.Brands = _productRepository.GetBrandsCombo();
                return View(model);
            }
            else
            {
                var product = await _productRepository.GetProductByIdAsync(model.Id);
                if (product == null)
                {
                    _toastNotification.Error("Error, the brand was not found");
                    model.Categories = _productRepository.GetCategoriesCombo();
                    model.Brands = _productRepository.GetBrandsCombo();
                    return View(model);
                };

                try
                {
                    // UPDATE
                    List<ProductImages> productImagesTemp = new List<ProductImages>();
                    product.Id = model.Id;

                    Guid imageId = Guid.Empty;
                    if (model.UploadFiles != null && model.UploadFiles.Count > 0)
                    {
                        foreach (var file in model.UploadFiles)
                        {
                            if (file.Length > 0)
                            {
                                using var image = Image.Load(file.OpenReadStream());
                                image.Mutate(img => img.Resize(512, 0));

                                using (MemoryStream m = new MemoryStream())
                                {
                                    image.SaveAsJpeg(m);
                                    byte[] imageBytes = m.ToArray();
                                    imageId = await _blobHelper.UploadBlobAsync(imageBytes, "products");
                                }

                                productImagesTemp.Add(new ProductImages
                                {
                                    ImageId = imageId,
                                });
                            }
                        }
                    }

                    //converterHelper
                    List<ProductImages> productImages = new List<ProductImages>();
                    foreach (var file in product.Images)
                    {
                        productImages.Add(new ProductImages
                        {
                            //Id = file.Id,
                            ImageId = file.ImageId,
                        });
                    }
                    if (productImagesTemp.Count > 0) 
                        productImages.AddRange(productImagesTemp);

                    product.Images = productImages;

                    product.Id = model.Id;
                    product.Name = model.Name;
                    product.Price = model.Price;
                    product.Description = model.Description;
                    product.IsService = model.IsService;
                    product.IsPromotion = model.IsPromotion;
                    product.CategoryId = Convert.ToInt32(model.CategoryId);
                    product.BrandId = Convert.ToInt32(model.BrandId);
                    
                    _dataContext.Products.Update(product);
                    await _dataContext.SaveChangesAsync();

                    //converterHelper
                    model = _converterHelper.ProductToViewModel(product);

                    _toastNotification.Success("Product changes saved successfully!!!");
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
                    model.Categories = _productRepository.GetCategoriesCombo();
                    model.Brands = _productRepository.GetBrandsCombo();
                    return View(model);
                }
            };

            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int? id)
        {
            var product = await _productRepository.GetServiceByIdAsync(id.Value);
            var model = new ServiceViewModel();
            if (product != null)
            {
                //converterHelper
                model = _converterHelper.ServiceToViewModel(product);
            }
            else
            {
                _toastNotification.Error("Service could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(ServiceViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.Categories = _productRepository.GetCategoriesCombo();
                model.Brands = _productRepository.GetBrandsCombo();
                return View(model);
            }
            else
            {
                var product = await _productRepository.GetServiceByIdAsync(model.Id);
                if (product == null)
                {
                    _toastNotification.Error("Error, the service was not found");
                    model.Categories = _productRepository.GetCategoriesCombo();
                    model.Brands = _productRepository.GetBrandsCombo();
                    return View(model);
                };

                try
                {
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
                    model = _converterHelper.ServiceToViewModel(product);

                    _toastNotification.Success("Brand changes saved successfully!!!");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        _toastNotification.Error($"The service  {model.Name}  already exists!");
                    }
                    else
                    {
                        _toastNotification.Error($"There was a problem updating the service, try again later!");
                    }

                    model.Categories = _productRepository.GetCategoriesCombo();
                    model.Brands = _productRepository.GetBrandsCombo();
                    return View(model);
                }
            };

            return View(model);
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Products/ProductDetails")]
        public async Task<JsonResult> ProductDetails(int? Id)
        {
            if (Id == null)
            {
                _toastNotification.Error("Product Id was not found.");
                return null;
            }

            var product = await _productRepository.GetProSerByIdAsync(Id.Value);
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
        [Route("Products/ToastNotification")]
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
