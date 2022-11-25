using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class Status : IEntity
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
