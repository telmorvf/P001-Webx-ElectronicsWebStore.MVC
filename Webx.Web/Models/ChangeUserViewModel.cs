using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class ChangeUserViewModel : User
    {
        public bool HasPassword { get; set; }
    }
}
