using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        private readonly DataContext _context;

        public StoreRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
        {
            //var storeAll = new List<Store>();
            IEnumerable<Store> storeAll;

            storeAll = await _context.Stores
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.Id)
                .ToListAsync();

            return storeAll;
        }

        public async Task<IEnumerable<Store>> GetAllStoresAsync()
        {
            IEnumerable<Store> storeAll;

            storeAll = await _context.Stores
                .OrderBy(s => s.Id)
                .ToListAsync();

            return storeAll;
        }

        public async Task<Store> GetAllStoreByIdAsync(int id)
        {
            var storeAll = await _context.Stores.Where(s => s.Id == id).FirstOrDefaultAsync();

            return storeAll;
        }

        public async Task<Store> GetAllStoreByNameAsync(string name)
        {
            var storeAll = await _context.Stores.Where(s => s.Name == name).FirstOrDefaultAsync();

            return storeAll;
        }

        public async Task AddStoreAsync(StoreViewModel model)
        {
            Store store = new Store
            {
                Name = model.Name,
                Address = model.Address,
                City = model.City,
                ZipCode = model.ZipCode,
                Country = model.Country,    
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsOnlineStore = model.IsOnlineStore,
                IsActive = model.IsActive,
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStoreAsync(StoreViewModel model)
        {
            Store store = new Store
            {
                Name = model.Name,
                Address = model.Address,
                City = model.City,
                ZipCode = model.ZipCode,
                Country = model.Country,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsOnlineStore = model.IsOnlineStore,
                IsActive = model.IsActive,
            };

            _context.Stores.Update(store);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<SelectListItem> GetComboStores()
        {
            var list = _context.Stores.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            }).OrderBy(s => s.Text);

            return list;
        }

        public async Task<int> GetOnlineStoreIdAsync()
        {
            var store = await _context.Stores.Where(s => s.Name.Contains("Online") && s.IsOnlineStore == true).FirstOrDefaultAsync();

            return store.Id;
        }
    }
}
