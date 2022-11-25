using System.Threading.Tasks;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public interface IPdfHelper
    {
        Task<string> PrintPDFAsync(InvoiceViewModel model);
    }
}
