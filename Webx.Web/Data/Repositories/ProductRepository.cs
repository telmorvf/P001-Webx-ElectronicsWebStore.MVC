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

        /// <summary>
        /// Return all products to the Shop
        /// </summary>
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

        /// <summary>
        /// Return all product to the Views of Product CRUD Controller
        /// </summary>
        public async Task<IEnumerable<Product>> GetAllProductsControllerAsync()
        {
            IEnumerable<Product> productAll;

            productAll = await _context.Products
                .OrderBy(p => p.Id)
                .ToListAsync();

            return productAll;
        }
    }
}
