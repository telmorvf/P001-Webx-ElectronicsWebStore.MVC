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

        async Task<List<Category>> ICategoryRepository.GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        async Task<Category> ICategoryRepository.GetAllCategoriesByIdAsync(int id)
        {
            return await _context.Categories.SingleOrDefaultAsync(c => c.Id == id);
        }

        async Task<Category> ICategoryRepository.GetAllCategoryByNameAsync(string name)
        {
            var categoryAll = await _context.Categories.Where(s => s.Name == name).FirstOrDefaultAsync();

            return categoryAll;
        }

        async Task ICategoryRepository.AddCategoryAsync(CategoryViewModel model)
        {
            Category category = new Category
            {
                Name = model.Name,
                ImageId = model.ImageId,
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

    }
}
