using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Webx.Web.Data.Entities
{
    public class Store : IEntity
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Store Name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "The field {0} cannot have more then {1} characters.")]
        public string Address { get; set; }
        
        [Required]
        public string City { get; set; }

        [StringLength(8, MinimumLength = 8)]
        [RegularExpression("(^\\d{4}(-\\d{3})?$)", ErrorMessage = "Format 9999-999")] // PT
        [Required(ErrorMessage = "9999-999")]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression("([0-9]{9})", ErrorMessage = "Please insert 9 digits format")] // PT
        public string PhoneNumber { get; set; }

        [Display(Name = "Online Store?")]
        public bool IsOnlineStore { get; set; }

        [Display(Name = "Active Store?")]
        public bool IsActive { get; set; }

        public string ImageFullPath
        {
            get => (IsOnlineStore == true)
            ? $"https://webx2022.blob.core.windows.net/images/store-online-round.png"
            : $"https://webx2022.blob.core.windows.net/images/store-round.png";
        }

        
    }
}
