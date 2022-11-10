using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        IEnumerable<SelectListItem> GetComboProdBrands();

        Task<Product> GetFullProduct(int id);
      
        Task<List<Product>> GetAllProducts(string category);

        Task<List<Product>> GetFilteredProducts(string category, List<string> brandsFilter);
        Task<decimal> MostExpensiveProductPriceAsync();
    }
}
