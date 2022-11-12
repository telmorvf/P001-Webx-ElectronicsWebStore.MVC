using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Webx.Web.Data.Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        
        [Required]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }
        
        [Required]
        public Brand Brand { get; set; }
        
        [Required]
        public Category Category { get; set; }

        public IEnumerable<ProductImages> Images { get; set; }

        public string Image => Images.Count() > 0 ? Images.ElementAt(0).ImageFullPath : "https://webx2022.blob.core.windows.net/images/NoPhoto.jpg";       
        
        [Required]
        public bool IsService { get; set; }
    }
}
