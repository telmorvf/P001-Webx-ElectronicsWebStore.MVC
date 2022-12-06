using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Webx.Web.Data.Entities;
using Webx.Web.Models;
using Webx.Web.Models.AdminPanel;

namespace Webx.Web.Data.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly DataContext _context;

        public CategoryRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Category> GetAllCategoriesByIdAsync(int id)
        {
            return await _context.Categories.SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> GetAllCategoryByNameAsync(string name)
        {
            var categoryAll = await _context.Categories.Where(s => s.Name == name).FirstOrDefaultAsync();

            return categoryAll;
        }

        public async Task AddCategoryAsync(CategoryViewModel model)
        {
            Category category = new Category
            {
                Name = model.Name,
                ImageId = model.ImageId,
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<List<ChartCategoriesViewModel>> GetMostSoldCategoriesData()
        {
            List<ChartCategoriesViewModel> list = new List<ChartCategoriesViewModel>();
            Random r = new Random();

            var thisMonthOrders = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .ThenInclude(o => o.Category)
                .Where(o => o.Order.OrderDate.Month == DateTime.UtcNow.Month && o.Product.Category.Name != "Services")
                .ToListAsync();

            var categories = await _context.Categories.ToListAsync();

            foreach (var category in categories)
            {
                int quantity = 0;

                foreach (var order in thisMonthOrders)
                {
                    //foreach (var categ in order.Product.Category)
                    //{
                    if (order.Product.Category != null)
                    {
                        if (order.Product.Category.Name == category.Name)
                        {
                            quantity++;
                        }
                    }
                    //}
                }

                if (quantity > 0)
                {
                    list.Add(new ChartCategoriesViewModel
                    {
                        Name = category.Name,
                        Quantity = quantity,
                        Color = String.Format("#{0:X6}", r.Next(0x1000000)),
                    });
                }

            }

            return list.OrderBy(l => l.Quantity).Take(5).ToList();
        }
    }
}
