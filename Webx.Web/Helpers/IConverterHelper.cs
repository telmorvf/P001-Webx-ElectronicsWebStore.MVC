using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public interface IConverterHelper
    {
        Task<EditEmployeeViewModel> ToEditEmployeeViewModelAsync(User user);
        EditCustomerViewModel ToEditCustomerViewModel(User user);
        StoreViewModel StoreToViewModel(Store store);
        Store StoreFromViewModel(StoreViewModel model, bool isNew);
        BrandViewModel BrandToViewModel(Brand brand);
        Brand BrandFromViewModel(BrandViewModel model, bool isNew);
        CategoryViewModel CategoryToViewModel(Category category);
        Category CategoryFromViewModel(CategoryViewModel model, bool isNew);
        ProductViewModel ProductToViewModel(Product product);
        Product ProductFromViewModel(ProductViewModel model, bool isNew);      
        StockViewModel StockToViewModel(Stock stock);
        Stock StockFromViewModel(StockViewModel model, bool isNew);
        Product ProductAddFromViewModel(ProductAddViewModel model, bool isNew);
        ProductAddViewModel ProductAddToViewModel(Product product);
        ServiceViewModel ServiceToViewModel(Product product);
        Task<Product> ServiceFromViewModel(ServiceViewModel model, bool isNew);          
        Task<List<CartViewModel>> ToCartViewModelAsync(List<CookieItemModel> cookieItemList);
        OrderViewModel ToOrderViewModel(Order order);
        Task<List<ProductWithReviewsViewModel>> ToProductsWithReviewsViewModelList(List<Product> products);
    }
}
