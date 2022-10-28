using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Helpers
{
    public interface IMailHelper
    {
        Task<Response> SendConfirmationEmail(string to,string tokenLink,User customer,string returnLink);
        Task<Response> SendResetPasswordEmail(string to, string link, User customer, string returnLink);
    }
}
