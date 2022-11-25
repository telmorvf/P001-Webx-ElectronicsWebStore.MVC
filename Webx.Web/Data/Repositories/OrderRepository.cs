using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly DataContext _context;

        public OrderRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddOrderAsync(OrderViewModel model,int storeId)
        {
            Order order1 = new Order
            {
                 Appointment = model.Appointment,
                 Customer = model.Customer,
                 DeliveryDate = model.DeliveryDate,
                 InvoiceId = model.InvoiceId,
                 OrderDate = model.OrderDate,
                 Status = model.Status,               
                 TotalPrice = model.TotalPrice,
                 Store = await _context.Stores.FindAsync(storeId),
                 TotalQuantity = model.TotalQuantity
            };

            _context.Orders.Add(order1);
            await _context.SaveChangesAsync();
        }

        public async Task<Response> CreateOrderDetailsAsync(List<OrderDetail> orderDetails)
        {
            try
            {
               await _context.OrderDetails.AddRangeAsync(orderDetails);
               await _context.SaveChangesAsync();
               return new Response { IsSuccess = true };
            }
            catch (Exception ex )
            {
                return new Response { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<List<OrderWithDetailsViewModel>> GetAllCustomerOrdersAsync(string customerId)
        {
            List<OrderWithDetailsViewModel> ordersWithDetails = new List<OrderWithDetailsViewModel>();
            List<Order> orders = new List<Order>();

            orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.Status)
                .Include(o => o.Appointment)
                .Where(o => o.Customer.Id == customerId).ToListAsync();

            foreach(var order in orders)
            {
                List<OrderDetail> orderDetails = new List<OrderDetail>();

                orderDetails = await _context.OrderDetails
                    .Include(od => od.Product)
                        .ThenInclude(p => p.Images)                    
                    .Where(od => od.Order.Id == order.Id).ToListAsync();

                ordersWithDetails.Add(new OrderWithDetailsViewModel
                {
                    Order = order,
                    OrderDetails = orderDetails
                });
            }

            return ordersWithDetails;
        }

        public async Task<Order> GetCompleteOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.Status)
                .Include(o => o.Appointment)
                .Where(o => o.Id == orderId).FirstOrDefaultAsync();
        }

        public async Task<List<Order>> GetCustomerRecentOrdersAsync(User user, DateTime date)
        {
            return await _context.Orders.Include(o=>o.Store).Where(o => o.Customer == user && o.OrderDate == date && o.DeliveryDate == date.AddDays(3)).ToListAsync();
        }

        public async Task<List<OrderDetail>> GetOrderDetailsAsync(int id)
        {
            return await _context.OrderDetails.Include(od => od.Product).ThenInclude(p => p.Images).Include(od=> od.Order).Where(od => od.Order.Id == id).ToListAsync();
        }

        public async Task<Status> GetOrderStatusByNameAsync(string orderStatusName)
        {
            return await _context.Statuses.Where(os => os.Name == orderStatusName).FirstOrDefaultAsync();
        }
    }
}
