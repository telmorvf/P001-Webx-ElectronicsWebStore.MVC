using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Webx.Web.Data.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [MaxLength(100, ErrorMessage = "The field Address cannot have more then 100 characters.")]
        public string Address { get; set; }

        [RegularExpression("^[0-9]*$", ErrorMessage = "Tax Identification Number must be numeric")]
        [MaxLength(9, ErrorMessage = "The Tax Identification Number field must have 9 characters.")]
        [MinLength(9, ErrorMessage = "The Tax Identification Number field must have 9 characters.")]
        [Display(Name = "Tax Identification number (NIF)")]
        public string NIF { get; set; }

        [Display(Name = "Profile Picture")]
        public Guid ImageId { get; set; }

        public bool Active { get; set; }        

        [Display(Name = "Phone Number")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone Number must be numeric")]
        [DataType(DataType.PhoneNumber)]
        public override string PhoneNumber { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [DataType(DataType.EmailAddress)]
        public override string Email { get; set; }

         public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://webx2022.blob.core.windows.net/images/NoPhoto-round.jpg"
            : $"https://webx2022.blob.core.windows.net/users/{ImageId}";

    }
}
