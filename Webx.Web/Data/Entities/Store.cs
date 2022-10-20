using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class Store : IEntity
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "The field {0} cannot have more then {1} characters.")]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string ZipCode { get; set; }
        [Required]
        public string Country { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public bool IsOnlineStore { get; set; }

        public bool IsActive { get; set; }



    }
}
