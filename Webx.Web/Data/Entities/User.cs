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

        [MaxLength(100, ErrorMessage = "The field {0} cannot have more then {1} characters.")]
        public string Address { get; set; }

        public long NIF { get; set; }

        [Display(Name = "Profile Picture")]
        public Guid ImageId { get; set; }

        public bool Active { get; set; }        

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public override string PhoneNumber { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

         public string ImageFullPath => ImageId == Guid.Empty
    ? $"https://webx2022.blob.core.windows.net/images/NoPhoto-round.jpg"
    : $"https://webx2022.blob.core.windows.net/users/{ImageId}";

    }
}
