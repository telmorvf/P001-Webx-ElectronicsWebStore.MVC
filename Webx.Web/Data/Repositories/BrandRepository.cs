
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using Webx.Web.Data.Entities;
using Webx.Web.Models;
using Webx.Web.Models.AdminPanel;

namespace Webx.Web.Data.Repositories
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        private readonly DataContext _context;

        public BrandRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brands.ToListAsync();
        }

        public async Task<Brand> GetBrandByNameAsync(string name)
        {
            return await _context.Brands.SingleOrDefaultAsync(b => b.Name == name);
        }
        
        public async Task<Brand> GetBrandByIdAsync(int id)
        {
            return await _context.Brands.SingleOrDefaultAsync(b => b.Id == id);
        }

        public async Task<int> GetTotalBrandsSoldAsync()
        {
            var brands = await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.IsService == false)
                .ToListAsync();

            return brands.Count;
        }

        public async Task<List<ChartBrandsViewModel>> GetBrandsChartDataAsync()
        {
            List<ChartBrandsViewModel> list = new List<ChartBrandsViewModel>();

            var products = await _context.Products.Include(p => p.Brand).Where(p => p.IsService == false).ToListAsync();
            var brands = await _context.Brands.Where(p => p.Name != "WebX").ToListAsync();
            Random r = new Random();
            var productsTotal = products.Count;

            foreach (var brand in brands)
            {
                var quantity = 0;

                foreach (var product in products)
                {
                    if (product.Brand.Name == brand.Name)
                    {
                        quantity++;
                    }
                }

                var percentage = ((double)quantity / (double)productsTotal).ToString("p1");

                if (quantity > 0)
                {
                    list.Add(new ChartBrandsViewModel
                    {
                        Brand = brand.Name,
                        Quantity = quantity,
                        Color = String.Format("#{0:X6}", r.Next(0x0800000)),
                        TotalBrands = products.Count,
                        Percentage = percentage,
                    });
                }
            }
            return list;
        }
    }
}
