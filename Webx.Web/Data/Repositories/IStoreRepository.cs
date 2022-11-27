using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public interface IStoreRepository : IGenericRepository<Store>
    {
        //Task AddStoreAsync(StoreViewModel model);

        Task<IEnumerable<Store>> GetAllActiveStoresAsync();

        Task<Store> GetAllStoreByIdAsync(int id);

        Task<Store> GetAllStoreByNameAsync(string name);

        Task<IEnumerable<Store>> GetAllStoresAsync();

        //Task UpdateStoreAsync(StoreViewModel model);        

        IEnumerable<SelectListItem> GetComboStores();

        IEnumerable<SelectListItem> GetComboPhysicalStores();

        Task<int> GetOnlineStoreIdAsync();
        Task<int> GetLisbonStoreIdAsync();
    }
}
