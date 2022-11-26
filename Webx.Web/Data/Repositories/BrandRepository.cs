
﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

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

    }
}
