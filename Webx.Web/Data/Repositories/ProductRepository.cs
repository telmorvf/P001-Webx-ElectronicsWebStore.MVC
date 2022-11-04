using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly DataContext _context;

        public ProductRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<SelectListItem> GetComboProdBrands()
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

#nullable enable
        public async Task<IEnumerable<Product>> GetFullProducts(string? category)
        {
            IEnumerable<Product> productAll;

            if (category == "AllCategories")
            {
                productAll =
                    await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Brand)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            else
            {
                productAll =
                    await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.Brand)
                    .Where(c => c.Category.Name == category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            return productAll;
        }
#nullable disable


    }
}
