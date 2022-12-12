namespace Webx.Web.Data.Entities
{
    public class ProductReviewTemps : IEntity
    {
        public int Id { get; set; }

        public ProductReview ProductReview { get; set; }
    }
}
