using System.Collections.Generic;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }

        public Order Order { get; set; }

        public List<OrderDetail> orderDetails { get; set; }
    }
}
