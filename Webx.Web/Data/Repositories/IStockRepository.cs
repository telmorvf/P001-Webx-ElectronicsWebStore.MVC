using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IStockRepository : IGenericRepository<Stock>
    {

        Task<List<Stock>> GetStockAlerts();
        Task<int> GetStockAlertsCount();
        Task<IEnumerable<Stock>> GetStockAllAsync();
        Task<IEnumerable<Stock>> GetStockAsync();
        Task<Stock> GetStockByIdAsync(int id);
        Task<List<Stock>> GetAllStockWithStoresAsync();
        Task<string> GetProductStockColorFromStoreIdAsync(int productId, int storeId);
        Task<Stock> GetProductStockInStoreAsync(int productId, int storeId);
     

    }
}
