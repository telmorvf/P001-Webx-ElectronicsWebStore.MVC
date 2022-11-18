using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsControllerAsync();

        IEnumerable<SelectListItem> GetBrandsCombo();
        IEnumerable<SelectListItem> GetBrandsCombo(int brandId);
        IEnumerable<SelectListItem> GetCategoriesCombo();
        IEnumerable<SelectListItem> GetCategoriesCombo(int categoryId);
        Task<Product> GetFullProduct(int id);

#nullable enable
        Task<IEnumerable<Product>> GetFullProducts(string? category);
#nullable disable

        Task<IEnumerable<Product>> GetProductAllAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> GetProductByNameAsync(string name);
        Task<Product> GetProSerByIdAsync(int id);
        Task<IEnumerable<Product>> GetServiceAllAsync();
        Task<Product> GetServiceByIdAsync(int id);
        Task<Product> GetServiceByNameAsync(string name);
    }
}
