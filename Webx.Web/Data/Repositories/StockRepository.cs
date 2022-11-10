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

        public async Task<IEnumerable<Stock>> GetAllStockAsync()
        {
            IEnumerable<Stock> stockAll;

            stockAll = await _context.Stocks
                .OrderBy(s => s.Product)
                .ThenBy(s => s.Store)
                .ToListAsync();

            return stockAll;
        }



    }
}
