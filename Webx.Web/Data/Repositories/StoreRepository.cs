using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        private readonly DataContext _context;

        public StoreRepository(DataContext context) : base(context)
        {
            _context = context;
        }
    }
}
