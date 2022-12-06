using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class ProductWithReviewsViewModel
    {
        public Product Product { get; set; }
        
        public int ProductOverallRating { get; set; }
    }
}
