using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository
            )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
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

                        // TODO: Falta Imagens

                        // TODO: Falta Stock

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

        public IActionResult ProductNotFound()
        {
            //TODO: View with a nice look, search in the net
            return View();
        }

        /// <summary>
        /// CRUD View All Products
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Product> products;

            // Get all Brands from the company:
            products = await _productRepository.GetAllProductsControllerAsync();

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Product);

            return View(products);
        }
    }
}
