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
    }
}
