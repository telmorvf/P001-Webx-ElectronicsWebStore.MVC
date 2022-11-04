using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Webx.Web.Data.Entities;
namespace Webx.Web.Models
{
    public class EditEmployeeViewModel : User
    {        
        [Display(Name = "Role")]        
        public string RoleId { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }

        [Display(Name ="Current Role")]
        public string CurrentRole { get; set; }
               
        public IFormFile PictureFile { get; set; }
    }
}
