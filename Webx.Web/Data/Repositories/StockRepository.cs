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

        public async Task<List<Stock>> GetAllStockWithStoresAsync()
        {
            List<Stock> AllStock = new List<Stock>();

            AllStock = await _context.Stocks.Include(s => s.Store).Include(s => s.Product).ToListAsync();

            return AllStock;

        } 

        public async Task<string> GetProductStockColorFromStoreIdAsync(int productId, int storeId)
        {
            var product = await _context.Products.Where(p => p.Id == productId).FirstOrDefaultAsync();
            var color = "";
            if (product.IsService)
            {
                color = "Green";
                return color;
            }
            else
            {
                var stock = await _context.Stocks.Where(s => s.Product.Id == productId && s.Store.Id == storeId).FirstOrDefaultAsync();
                var productTotal = stock.Quantity;

                if(productTotal < 10)
                {
                    color = "Red";
                }

                if(productTotal >= 10 && productTotal <=25)
                {
                    color = "#ffb703";
                }

                if(productTotal > 25)
                {
                    color = "Green";
                }

                return color;
            }
        }

        public async Task<Stock> GetProductStockInStoreAsync(int productId, int storeId)
        {
            var stock = await _context.Stocks.Where(s => s.Product.Id == productId && s.Store.Id == storeId).FirstOrDefaultAsync();

            return stock;
        }
    }
}
