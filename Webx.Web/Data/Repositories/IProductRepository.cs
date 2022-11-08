using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsControllerAsync();

        IEnumerable<SelectListItem> GetComboProdBrands();

        Task<Product> GetFullProduct(int id);

        #nullable enable
        Task<IEnumerable<Product>> GetFullProducts(string? category);
        #nullable disable
    
    
    
    }
}
