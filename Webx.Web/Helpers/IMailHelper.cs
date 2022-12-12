using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Helpers
{
    public interface IMailHelper
    {
        Task<Response> SendConfirmationEmail(string to,string tokenLink,User customer,string returnLink);
        Task<Response> SendResetPasswordEmail(string to, string link, User customer, string returnLink);
        Task<Response> SendEmployeeConfirmationEmail(string email, string tokenLink, User user, string returnLink);
        Task<Response> SendEmailWithInvoicesAsync(string to, List<string> attachments,User user,string paymentId);
        List<SelectListItem> Destinations();
        Task<Response> SendAnnouncement(string to, string subject, string body, string attachment);
        Task<Response> SendAnnouncementAsync(int to, string subject, string body, string path);
    }
}
