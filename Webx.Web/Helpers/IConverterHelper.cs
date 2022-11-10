using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public interface IConverterHelper
    {
        Task<EditEmployeeViewModel> ToEditEmployeeViewModelAsync(User user);

        StoreViewModel StoreToViewModel(Store store);
        Store StoreFromViewModel(StoreViewModel model, bool isNew);

        BrandViewModel BrandToViewModel(Brand brand);
        Brand BrandFromViewModel(BrandViewModel model, bool isNew);

        CategoryViewModel CategoryToViewModel(Category category);
        Category CategoryFromViewModel(CategoryViewModel model, bool isNew);

        ProductViewModel ProductToViewModel(Product product);

        StockViewModel StockToViewModel(Stock stock);

        EditCustomerViewModel ToEditCustomerViewModel(User user);

        
        
    }
}
