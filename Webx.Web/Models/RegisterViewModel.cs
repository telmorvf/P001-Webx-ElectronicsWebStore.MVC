using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Webx.Web.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [MaxLength(100, ErrorMessage = "The field {0} can only contain {1} characters.")]
        public string Address { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "{0} must be numeric")]
        [Display(Name = "Phone Number")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [EmailAddress]
        public string UserName { get; set; }
        
        [RegularExpression("^[0-9]*$", ErrorMessage = "{0} must be numeric")]
        [MaxLength(9, ErrorMessage = "The {0} field must have {1} characters.")]
        [MinLength(9, ErrorMessage = "The {0} field must have {1} characters.")]
        [Display(Name = "Tax Identification number (NIF)")]
        public string NIF { get; set; }
    }
}
