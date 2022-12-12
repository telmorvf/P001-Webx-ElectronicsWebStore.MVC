using System.Linq;
using Webx.Web.Data.Entities;
using static System.Net.WebRequestMethods;

namespace Webx.Web.Models
{
    public class CartViewModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public int StoreId { get; set; }      
        public string Color { get; set; }
        public string Image => Product.Images.Count() > 0 ? Product.Images.ElementAt(0).ImageFullPath : "https://webx2022.blob.core.windows.net/images/NoPhoto.jpg";

        public string TotalPrice => (Product.Price * Quantity).ToString("C2");

        public string TotalPriceWithDiscount => (Product.PriceWithDiscount * Quantity).ToString("C2");

        public string Price => Product.Price.ToString("C2");
    }
}
