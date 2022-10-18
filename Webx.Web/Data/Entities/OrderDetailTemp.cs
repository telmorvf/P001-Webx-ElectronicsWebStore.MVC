namespace Webx.Web.Data.Entities
{
    public class OrderDetailTemp : IEntity
    {
        public int Id { get; set; }

        public User User { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

    }
}
