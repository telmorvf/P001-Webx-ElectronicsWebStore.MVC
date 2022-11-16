using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Webx.Web.Data.Entities
{
    public class Stock : IEntity
    {
        public int Id { get; set; }

        public Product Product { get; set; }
        public int ProductId { get; set; }

        public Store Store { get; set; }
        public int StoreId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Only positive number allowed.")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Minimum Quantity?")]
        [Range(0, int.MaxValue, ErrorMessage = "Only positive number allowed.")]
        public int MinimumQuantity { get; set; }

    }
}
