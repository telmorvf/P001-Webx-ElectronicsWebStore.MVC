using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class Category : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
