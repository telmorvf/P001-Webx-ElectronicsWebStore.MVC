using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public interface IConverterHelper
    {
        Task<EditEmployeeViewModel> ToEditEmployeeViewModelAsync(User user);

        public StoreViewModel StoreToViewModel(Store store);

        BrandViewModel BrandToViewModel(Brand brand);

        CategoryViewModel CategoryToViewModel(Category category);

        ProductViewModel ProductToViewModel(Product product);

        StockViewModel StockToViewModel(Stock stock);

        EditCustomerViewModel ToEditCustomerViewModel(User user);

    }
}
