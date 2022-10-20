namespace Webx.Web.Data.Entities
{
    public class OrderDetail : IEntity
    {
        public int Id { get; set; }

        public Order Order { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

    }
}
