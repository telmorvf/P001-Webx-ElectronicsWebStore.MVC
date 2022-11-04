using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public interface IConverterHelper
    {
        Task<EditEmployeeViewModel> ToEditEmployeeViewModelAsync(User user);
    }
}
