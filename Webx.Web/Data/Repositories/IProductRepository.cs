using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsControllerAsync();

        IEnumerable<SelectListItem> GetBrandsCombo();
        IEnumerable<SelectListItem> GetBrandsCombo(int brandId);
        IEnumerable<SelectListItem> GetCategoriesCombo();
        Task<int> GetReviewsTempsCountAsync();
        IEnumerable<SelectListItem> GetCategoriesCombo(int categoryId);
        Task<Product> GetFullProduct(int id);      
        Task<List<Product>> GetAllProducts(string category);
#nullable enable
        Task<IEnumerable<Product>> GetFullProducts(string? category);
#nullable disable
        Task<IEnumerable<Product>> GetProductAllAsync();
        Task<Product> GetProductByIdAsync(int id);      
        Task<Product> GetProSerByIdAsync(int id);
        Task<IEnumerable<Product>> GetServiceAllAsync();
        Task<Product> GetServiceByIdAsync(int id);
        Task<Product> GetServiceByNameAsync(string name);
        Task<List<Product>> GetFilteredProducts(string category, List<string> brandsFilter);
        Task<decimal> MostExpensiveProductPriceAsync();       
        Task<ShopViewModel> GetInitialShopViewModelAsync();
        Task<List<CartViewModel>> GetCurrentCartAsync();
        bool CheckCookieConsentStatus();
        Response UpdateCartCookie(List<CartViewModel> cart);
        Response ClearCart();
        Task<List<APIProductsModel>> GetProductsAsync();
        Task<Product> GetProductByNameAsync(string productName);
        Task<List<Product>> GetAllProductsAsync();
        Task<List<Product>> GetHighlightedProductsAsync();
        Task<List<Product>> GetOrStartWishListAsync();
        Response AddProductToWishList(Product product);
        Response UpdateWishlistCookie(List<Product> currentWishlist);
        Task<List<ProductReview>> GetProductReviewsAsync(int productId);
        Task<ProductReview>  GetThisCustomerProdReviewAsync(User user, Product product);
        Task CreateReviewAsync(ProductReview review);
        Task<ProductReview> GetProductReviewByIdAsync(int value);
        Task UpdateReviewAsync(ProductReview customerReview);
        Task<List<ProductReview>> GetAllReviewsAsync();
        Task<ProductReview> GetRecentCreatedReviewAsync(ProductReview review);
        Task CreateReviewTempAsync(ProductReview review);
        Task RemoveReviewTempIfExistsAsync(ProductReview customerReview);
        Task<List<ProductReviewTemps>> GetReviewsTempsAsync();
        Task RemoveReviewTempsAsync(List<ProductReviewTemps> temps);

    }
}
