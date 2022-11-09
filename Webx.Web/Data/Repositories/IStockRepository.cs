using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IStockRepository : IGenericRepository<Stock>
    {
        Task<IEnumerable<Stock>> GetAllStockAsync();
    }
}
