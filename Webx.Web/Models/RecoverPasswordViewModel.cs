using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }
}
