namespace Webx.Web.Data.Entities
{
    public class Stock : IEntity
    {
        public int Id { get; set; }

        public Product Product { get; set; }

        public Store Store { get; set; }

        public int Quantity { get; set; }

        public int MinimumQuantity { get; set; }

    }
}
