using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Helpers
{
    public interface IMailHelper
    {
        Task<Response> SendConfirmationEmail(string to,string tokenLink,User customer);
        Task<Response> SendResetPasswordEmail(string to, string link, User customer);
    }
}
