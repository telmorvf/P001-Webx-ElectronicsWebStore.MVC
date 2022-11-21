using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IStockRepository : IGenericRepository<Stock>
    {
        Task<IEnumerable<Stock>> GetAllStockAsync();

        Task<List<Stock>> GetAllStockWithStoresAsync();

        Task<string> GetProductStockColorFromStoreIdAsync(int productId, int storeId);

        Task<Stock> GetProductStockInStoreAsync(int productId, int storeId);
     
    }
}
