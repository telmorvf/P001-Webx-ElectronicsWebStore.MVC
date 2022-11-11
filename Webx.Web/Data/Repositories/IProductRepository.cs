using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsControllerAsync();

        IEnumerable<SelectListItem> GetBrandsCombo();

        IEnumerable<SelectListItem> GetCategoriesCombo();

        Task<Product> GetFullProduct(int id);
      
        Task<List<Product>> GetAllProducts(string category);

        Task<List<Product>> GetFilteredProducts(string category, List<string> brandsFilter);
        Task<decimal> MostExpensiveProductPriceAsync();
       
        Task<ShopViewModel> GetInitialShopViewModelAsync();

        Task<List<CartViewModel>> GetCurrentCartAsync();
        bool CheckCookieConsentStatus();
    }
}
