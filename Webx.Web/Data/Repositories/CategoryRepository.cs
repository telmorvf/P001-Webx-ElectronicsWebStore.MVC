using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

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
    }
}
