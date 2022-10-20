using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public Brand Brand { get; set; }
        [Required]
        public Category Category { get; set; }

        public IEnumerable<ProductImages> Images { get; set; }
        [Required]
        public bool IsService { get; set; }
    }
}
