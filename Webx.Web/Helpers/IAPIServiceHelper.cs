using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public interface IAPIServiceHelper
    {
        Task<List<DistrictsViewModel>> GetDistrictsInfoAsync();

        public Task<IEnumerable<SelectListItem>> GetComboDistricts();

        public Task<IEnumerable<SelectListItem>> GetComboCounties(int districtId);
    }
}
