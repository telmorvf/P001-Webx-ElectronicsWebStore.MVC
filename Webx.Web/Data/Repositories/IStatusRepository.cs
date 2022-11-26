using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
namespace Webx.Web.Data.Repositories
{
    public interface IStatusRepository : IGenericRepository<Status>
    {
        Task<Status> GetStatusByIdAsync(string id);
        IEnumerable<SelectListItem> GetStatusesCombo();
    }
}
