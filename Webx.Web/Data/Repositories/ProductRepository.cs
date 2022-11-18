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

        public IEnumerable<SelectListItem> GetBrandsCombo(int brandId)
        {
            var brand = _context.Brands.Find(brandId);
            var list = new List<SelectListItem>();
            if (brand != null)
            {
                list = _context.Brands.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()

                }).OrderBy(l => l.Text).ToList();


                list.Insert(0, new SelectListItem
                {
                    Text = "(Select a brand...)",
                    Value = "0"
                });

            }

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

        public IEnumerable<SelectListItem> GetCategoriesCombo(int categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            var list = new List<SelectListItem>();
            if (category != null)
            {
                list = _context.Categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()

                }).OrderBy(l => l.Text).ToList();


                list.Insert(0, new SelectListItem
                {
                    Text = "(Select a category...)",
                    Value = "0"
                });

            }

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
                .OrderBy(p => p.Id)
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

        public async Task<IEnumerable<Product>> GetAllProductsControllerAsync()
        {
            IEnumerable<Product> productAll;

            productAll = await _context.Products
                .OrderBy(p => p.Id)
                .ToListAsync();

            return productAll;
        }


        public async Task<IEnumerable<Product>> GetProductAllAsync()
        {
            IEnumerable<Product> productAll;

            productAll = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Brand)
                .Where(p => p.IsService == false)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return productAll;
        }
        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product =
                await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Brand)
                .Where(p => p.Id == id && p.IsService == false)
                .OrderBy(p => p.Name)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return null;
            }
            return product;
        }
        public async Task<Product> GetProductByNameAsync(string name)
        {
            return await _context.Products.SingleOrDefaultAsync(b => b.Name == name);
        }

        public async Task<Product> GetProSerByIdAsync(int id)
        {
            var product =
                await _context.Products
                .Include(p => p.Category)
                //.Include(p => p.Images)
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

        public async Task<IEnumerable<Product>> GetServiceAllAsync()
        {
            IEnumerable<Product> productAll;

            productAll = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsService == true)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return productAll;
        }
        public async Task<Product> GetServiceByIdAsync(int id)
        {
            var product =
                await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Id == id && p.IsService == true)
                .OrderBy(p => p.Name)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return null;
            }
            return product;
        }
        public async Task<Product> GetServiceByNameAsync(string name)
        {
            return await _context.Products.SingleOrDefaultAsync(b => b.Name == name);
        }
    }
}
