using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class CreateEmployeeViewModel : User
    {
        [Display(Name = "Role")]
        [MinLength(3, ErrorMessage = "Must select a role.")]
        [Required]
        public string RoleId { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }

        public IFormFile PictureFile { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required]
        public override string Email { get; set; }
    }
}
