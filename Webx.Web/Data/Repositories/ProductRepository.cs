using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public ProductRepository(DataContext context,IHttpContextAccessor httpContext) : base(context)
        {
            _context = context;
            _httpContext = httpContext;       
        }

        public IEnumerable<SelectListItem> GetBrandsCombo()
        {
            var list = _context.Brands.Select(b => new SelectListItem
            {
                Text = b.Name,
                Value = b.Id.ToString()

            }).OrderBy(b => b.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a brand...)",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetCategoriesCombo()
        {
            var list = _context.Categories.Select(b => new SelectListItem
            {
                Text = b.Name,
                Value = b.Id.ToString()

            }).OrderBy(b => b.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "(Select a category...)",
                Value = "0"
            });

            return list;
        }

        /// <summary>
        /// Return all products to the Shop by id (when i click one product)
        /// </summary>
        public async Task<Product> GetFullProduct(int id)
        {
            var product =
                await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Brand)
                .Where(p => p.Id == id)
                .OrderBy(p => p.Name)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return null;
            }
            return product;
        }

        public async Task<List<Product>> GetAllProducts(string category)
        {
            //IEnumerable<Product> productAll;
            List<Product> list = new List<Product>();

            if (category == "AllCategories")
            {

                list =
                    await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Brand)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            else
            {
                list =
                    await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Brand)
                    .Where(c => c.Category.Name == category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }

            return list;
        }

        public async Task<List<Product>> GetFilteredProducts(string category, List<string> brandsFilter)
        {
            //IEnumerable<Product> productAll;
            List<Product> list = new List<Product>();

            if (category == "AllCategories")
            {

                list =
                    await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Brand)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            else
            {
                list =
                    await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Brand)
                    .Where(c => c.Category.Name == category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }

            if (brandsFilter != null && brandsFilter.Count > 0)
            {
                List<Product> filteredlist = new List<Product>();


                foreach (var filter in brandsFilter)
                {                    
                    filteredlist.AddRange(list.Where(p => p.Brand.HtmlId == filter).ToList());
                }

                return filteredlist;
            }

            return list;
        }


        public async Task<decimal> MostExpensiveProductPriceAsync()
        {
            var Product = await _context.Products.OrderByDescending(p => p.Price).FirstAsync();

            return Product.Price;
        }

        /// <summary>
        /// Return all product to the Views of Product CRUD Controller
        /// </summary>
        public async Task<IEnumerable<Product>> GetAllProductsControllerAsync()
        {
            IEnumerable<Product> productAll;

            productAll = await _context.Products
                .OrderBy(p => p.Id)
                .ToListAsync();


        public async Task<ShopViewModel> GetInitialShopViewModelAsync()
        {
            var cookie = _httpContext.HttpContext.Request.Cookies["Cart"];
            List<CartViewModel> cart = new List<CartViewModel>();            

            if (string.IsNullOrEmpty(cookie))
            {
                List<CookieItemModel> cookieItemList = new List<CookieItemModel>();
                var serializedCart = JsonConvert.SerializeObject(cookieItemList);
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.UtcNow.AddDays(365);
                options.Secure = true;
                _httpContext.HttpContext.Response.Cookies.Append("Cart", serializedCart, options);
            }
            else
            {
                var cookieItemList = JsonConvert.DeserializeObject<List<CookieItemModel>>(cookie);
                foreach (var item in cookieItemList)
                {
                    var product = await GetFullProduct(item.ProductId);
                    cart.Add(new CartViewModel { Product = product, Quantity = item.Quantity });                   
                }
            }             
           
            return new ShopViewModel{
                 Cart = cart,
            };          
        }

        public async Task<List<CartViewModel>> GetCurrentCartAsync()
        {
            List<CartViewModel> cart = new List<CartViewModel>();

            var cookie = _httpContext.HttpContext.Request.Cookies["Cart"];
            var cookieItemList = JsonConvert.DeserializeObject<List<CookieItemModel>>(cookie);
            if(cookieItemList != null && cookieItemList.Count() > 0)
            {
                foreach (var item in cookieItemList)
                {
                    var product = await GetFullProduct(item.ProductId);
                    cart.Add(new CartViewModel { Product = product, Quantity = item.Quantity });                   
                }
            }
            
            return cart;

        }

        public bool CheckCookieConsentStatus()
        {
            var cookieConsent = _httpContext.HttpContext.Request.Cookies["Consent"];
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.UtcNow.AddDays(365);
            options.Secure = true;

            if (string.IsNullOrEmpty(cookieConsent))
            {                
                _httpContext.HttpContext.Response.Cookies.Append("Consent", "false", options);
                return false;
            }
            else
            {
                _httpContext.HttpContext.Response.Cookies.Append("Consent", "true", options);
                return true;
            }        

        }
    }
}
