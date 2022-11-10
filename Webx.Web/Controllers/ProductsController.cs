using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;
using X.PagedList.Mvc;
using X.PagedList;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace Webx.Web.Controllers
{
    public class ProductsController : Controller
    {


        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly INotyfService _toastNotification;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            INotyfService toastNotification
            )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            // TO DO: Falta Stock

            //TO DO: Falta adicionar propriedade ProductsCount na Entidade Brand para que possa mostrar a quantidade
            //que existe de cada produto para a categoria especifica. O mesmo tem de ser implementado quando se programar para adiconar/remover produtos

            var products = await _productRepository.GetAllProducts("AllCategories");

            if(products == null)
            {
                _toastNotification.Error("There was a problem loading the store.Please try again later!");
                return NotFound();
            }

            var model = new ShopViewModel
            {
                PagedListProduct = products.ToPagedList(1,12),
                SelectedCategory = "AllCategories",
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                ResultsPerPage = 12,
                NumberOfProductsFound = products.Count(),
                Brands = await _brandRepository.GetAllBrandsAsync(),
                MostExpensiveProductPrice = await _productRepository.MostExpensiveProductPriceAsync(),
            };

            return View(model);
        }

        
        public async Task<IActionResult> Details(int? id)
        {
            var product = await _productRepository.GetFullProduct(id.Value);

            if (product == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            return View(product);
        }

        [HttpGet]
        public async Task<ActionResult> ClearFilters(int? resultsPerPage)
        {
            var products = await _productRepository.GetAllProducts("AllCategories");

           

            var model = new ShopViewModel
            {
                PagedListProduct = products.ToPagedList(1, resultsPerPage ?? 12),
                SelectedCategory = "AllCategories",
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                NumberOfProductsFound = products.Count(),
                Brands = await _brandRepository.GetAllBrandsAsync()
            };               
                
            return PartialView("_shopSectionPartial", model);
        }

        [HttpGet]
        public async Task<ActionResult> FilterBrand(string category, int? resultsPerPage,int minRange,int maxRange ,string brandsfilter)
        {       
            
            var brandsList = JsonConvert.DeserializeObject<List<string>>(brandsfilter);

            var products = await _productRepository.GetFilteredProducts(category, brandsList);

            if (products == null)
            {
                _toastNotification.Error("There was a problem loading the store.Please try again later!");
                return NotFound();
            }

            products = products.Where(p => p.Price >= minRange && p.Price <= maxRange).ToList();

            var model = new ShopViewModel
            {
                PagedListProduct = products.ToPagedList(1, resultsPerPage ?? 12),
                SelectedCategory = category,
                ResultsPerPage = resultsPerPage?? 12,
                BrandsTags = brandsList,
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                NumberOfProductsFound = products.Count(),
                Brands = await _brandRepository.GetAllBrandsAsync()
            };              

            return PartialView("_shopSectionPartial", model);
        }

        [HttpGet]
        public async Task<ActionResult> ChangeCategory(string category)
        {
            var products = await _productRepository.GetAllProducts(category);

            if (products == null)
            {
                _toastNotification.Error("There was a problem loading the store.Please try again later!");
                return NotFound();
            }

            var model = new ShopViewModel
            {
                PagedListProduct = products.ToPagedList(1, 12),
                SelectedCategory = category,
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                NumberOfProductsFound = products.Count(),
                Brands = await _brandRepository.GetAllBrandsAsync()
            };

            return PartialView("_shopSectionPartial", model);
        }

        [HttpGet]
        public async Task<ActionResult> ChangeResultsPerPage(string category, int resultsPerPage, string brandsfilter = null)
        {
            var brandsList = new List<string>();
            var products = new List<Product>();
            var categoryFilter = category.Replace(" ", "");

            if(brandsfilter != null && brandsfilter.Length > 0)
            {
                brandsList = JsonConvert.DeserializeObject<List<string>>(brandsfilter);
                products = await _productRepository.GetFilteredProducts(categoryFilter, brandsList);
            }
            else
            {
                products = await _productRepository.GetAllProducts(categoryFilter);
            }

            if (products == null)
            {
                _toastNotification.Error("There was a problem loading the store.Please try again later!");
                return NotFound();
            }

            var model = new ShopViewModel
            {
                PagedListProduct = products.ToPagedList(1, resultsPerPage),
                SelectedCategory = category,
                ResultsPerPage = resultsPerPage,
                NumberOfProductsFound = products.Count(),
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                Brands = await _brandRepository.GetAllBrandsAsync()
            };              

            return PartialView("_shopSectionPartial", model);
        }

        public async Task<IActionResult> ReturnDesiredProductPage(string category, int resultsPerPage, int? page, List<string> brandsFilter = null)
        {
            var products = new List<Product>();

            if (brandsFilter != null && brandsFilter.Count > 0)
            {               
                products = await _productRepository.GetFilteredProducts(category, brandsFilter);
            }
            else
            {
                products = await _productRepository.GetAllProducts(category);
            }

            var model = new ShopViewModel
            {
                PagedListProduct = products.ToPagedList(page?? 1, resultsPerPage),
                SelectedCategory = category,
                ResultsPerPage = resultsPerPage,
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                NumberOfProductsFound = products.Count(),
                Brands = await _brandRepository.GetAllBrandsAsync(),
                MostExpensiveProductPrice = await _productRepository.MostExpensiveProductPriceAsync(),
            };

            return View("Index", model);
        }


        [HttpGet]
        public async Task<IActionResult> ChangePriceRange(string category, int resultsPerPage, int minRange,int maxRange, string brandsFilter = null)
        {
            var products = new List<Product>();
            var brandsList = new List<string>();

            if (brandsFilter != null && brandsFilter.Length > 2)
            {
                brandsList = JsonConvert.DeserializeObject<List<string>>(brandsFilter);
                products = await _productRepository.GetFilteredProducts(category, brandsList);
                products = products.Where(p => p.Price >= minRange && p.Price <= maxRange).ToList();
            }
            else
            {
                products = await _productRepository.GetAllProducts(category);
                products = products.Where(p => p.Price >= minRange && p.Price <= maxRange).ToList();
            }


            var model = new ShopViewModel
            {
                PagedListProduct = products.ToPagedList(1, resultsPerPage),
                SelectedCategory = category,
                ResultsPerPage = resultsPerPage,
                NumberOfProductsFound = products.Count(),
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                Brands = await _brandRepository.GetAllBrandsAsync()
            };

            return PartialView("_shopSectionPartial", model);
        }

        [HttpGet]        
        public async Task<IActionResult> GetProductDetails(int? Id,int resultsPerPage, string category, int minRange, int maxRange, string brandsFilter = null)
        {
            if (Id == 0)
            {
                return null;
            }

            var product = await _productRepository.GetFullProduct(Id.Value);

            if (product == null)
            {
                return null;
            }

            var products = new List<Product>();
            var brandsList = new List<string>();

            if (brandsFilter != null && brandsFilter.Length > 2)
            {
                brandsList = JsonConvert.DeserializeObject<List<string>>(brandsFilter);
                products = await _productRepository.GetFilteredProducts(category, brandsList);
                products = products.Where(p => p.Price >= minRange && p.Price <= maxRange).ToList();
            }
            else
            {
                products = await _productRepository.GetAllProducts(category);
                products = products.Where(p => p.Price >= minRange && p.Price <= maxRange).ToList();
            }

            var model = new ShopViewModel
            {
                Product = product,
                PagedListProduct = products.ToPagedList(1, resultsPerPage),
                SelectedCategory = category,
                ResultsPerPage = resultsPerPage,
                NumberOfProductsFound = products.Count(),
                Categories = await _categoryRepository.GetAllCategoriesAsync(),
                Brands = await _brandRepository.GetAllBrandsAsync()
            };

            return PartialView("_ProductModalPartial",model);
        }


        //[HttpGet]
        //public async Task<IActionResult> CloseViewModal( int resultsPerPage, string category, int minRange, int maxRange, string brandsFilter = null)
        //{
        //    var products = new List<Product>();
        //    var brandsList = new List<string>();

        //    if (brandsFilter != null && brandsFilter.Length > 2)
        //    {
        //        brandsList = JsonConvert.DeserializeObject<List<string>>(brandsFilter);
        //        products = await _productRepository.GetFilteredProducts(category, brandsList);
        //        products = products.Where(p => p.Price >= minRange && p.Price <= maxRange).ToList();
        //    }
        //    else
        //    {
        //        products = await _productRepository.GetAllProducts(category);
        //        products = products.Where(p => p.Price >= minRange && p.Price <= maxRange).ToList();
        //    }

        //    var model = new ShopViewModel
        //    {                
        //        PagedListProduct = products.ToPagedList(1, resultsPerPage),
        //        SelectedCategory = category,
        //        ResultsPerPage = resultsPerPage,
        //        NumberOfProductsFound = products.Count(),
        //        Categories = await _categoryRepository.GetAllCategoriesAsync(),
        //        Brands = await _brandRepository.GetAllBrandsAsync()
        //    };

        //    return PartialView("_shopSectionPartial", model);
        //}


       public IActionResult ProductNotFound()
       {
       //TODO: View with a nice look, search in the net
       return View();
       }

    }
}
