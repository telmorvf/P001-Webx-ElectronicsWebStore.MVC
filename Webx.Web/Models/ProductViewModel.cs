using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class ProductViewModel : Product
    {
        public IEnumerable<SelectListItem> Brands { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

    }
}
