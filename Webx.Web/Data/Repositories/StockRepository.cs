using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;

using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public class StockRepository : GenericRepository<Stock>, IStockRepository
    {
        private readonly DataContext _context;

        public StockRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Stock>> GetStockAsync()
        {
            IEnumerable<Stock> stockAll;

            stockAll = await _context.Stocks
                .OrderBy(s => s.Product)
                .ThenBy(s => s.Store)
                .ToListAsync();

            return stockAll;
        }

        public async Task<IEnumerable<Stock>> GetStockAllAsync()
        {
            IEnumerable<Stock> stockAll;

            stockAll = await _context.Stocks
                .Include(p => p.Product)
                    .ThenInclude(p => p.Brand)
                .Include(p => p.Product)
                    .ThenInclude(p => p.Category)
                .Include(p => p.Product)
                    .ThenInclude(i => i.Images)
                .Include(p => p.Store)
                //.Where(p => p.Product.IsService == false)
                .OrderBy(p => p.Product.Name)
                .ThenBy(p => p.Store.Name)
                .ToListAsync();

            return stockAll;
        }

        public async Task<Stock> GetStockByIdAsync(int id)
        {
            return await _context.Stocks
                .Include(p => p.Product)
                    .ThenInclude(p => p.Brand)
                .Include(p => p.Product)
                    .ThenInclude(p => p.Category)
                .Include(p => p.Product)
                    .ThenInclude(i => i.Images)
                .Include(p => p.Store)
                .Where(p => p.Id == id)
                .OrderBy(p => p.Product.Id)
                .ThenBy(p => p.Store.Name)
                .SingleOrDefaultAsync();
        }


        public async Task<List<Stock>> GetStockAlerts()
        {
            var stocks = await _context.Stocks
                .Include(p => p.Product)
                    .ThenInclude(p => p.Brand)
                .Include(p => p.Product)
                    .ThenInclude(p => p.Category)
                .Include(p => p.Product)
                    .ThenInclude(i => i.Images)
                .Include(p => p.Store)
                .Where(p => p.Quantity < p.MinimumQuantity && p.Product.IsService == false)
                .ToListAsync();

            return stocks;
        }

        public async Task<int> GetStockAlertsCount()
        {
            var count = await _context.Stocks
                .Where(p => p.Quantity < p.MinimumQuantity && p.Product.IsService == false)
                .CountAsync();

            return count;
        }



    }
}
