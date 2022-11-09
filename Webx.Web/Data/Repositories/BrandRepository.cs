using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using Webx.Web.Data.Entities;

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
            IEnumerable<Brand> brandAll;

            brandAll = await _context.Brands
                .OrderBy(p => p.Id)
                .ToListAsync();

            return brandAll;
        }
    }
}
