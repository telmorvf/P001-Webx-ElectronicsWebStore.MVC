using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class ProductsAllViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Brand Brand { get; set; }
        public Category Category { get; set; }
        public bool IsService { get; set; }
    }
}
