using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly DataContext _context;

        public OrderRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            IEnumerable<Order> orders;

            orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.Appointment)
                .Include(o => o.Status)
                .OrderBy(o => o.Id)
                .ToListAsync();

            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders.Where(o => o.Id == id)
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.Appointment)
                .Include(o => o.Status)
                .FirstOrDefaultAsync();

            return order;
        }

        public async Task<IEnumerable<OrderDetail>> GetAllOrderDetailsByOrderIdAsync(int id)
        {
            IEnumerable<OrderDetail> orderDetails;

            orderDetails = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Order.Id == id)
                .ToListAsync();
            
            return orderDetails;
        }

        public IEnumerable<SelectListItem> GetStatusesCombo()
        {
            var list=new List<Status>();
            var comboList = new List<SelectListItem>();

            list.Add(_context.Statuses.Where(s => s.Name=="Order Closed").FirstOrDefault());
            list.Add(_context.Statuses.Where(s => s.Name == "Appointment Done").FirstOrDefault());
            list.Add(_context.Statuses.Where(s => s.Name == "Order Shipped").FirstOrDefault());

            foreach (var status in list)
            {
                  comboList.Add(new SelectListItem
                  {
                      Text = status.Name,
                      Value = status.Id.ToString(),
                  });
                
            }

            return comboList.OrderBy(l => l.Text);

        }
    }
}
