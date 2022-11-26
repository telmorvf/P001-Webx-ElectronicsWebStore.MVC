using System.Collections.Generic;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class OrderWithDetailsViewModel
    {
        public Order Order { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } 
    }
}
