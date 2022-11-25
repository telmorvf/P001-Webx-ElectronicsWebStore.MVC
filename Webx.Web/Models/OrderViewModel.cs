using Webx.Web.Data.Entities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Models
{
    public class OrderViewModel: Order
    {
        public IEnumerable<OrderDetail> OrderDetails { get; set; }

        public IEnumerable<SelectListItem> Statuses { get; set; }

        [Display(Name = "Status")]
        public string StatusId  { get; set; } 
    }
}
