using Webx.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Models
{
    public class CreateCustomerViewModel: User
    {
        public string RoleId { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required]
        public override string Email { get; set; }
    }
}
