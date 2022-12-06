using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;
using Webx.Web.Models.AdminPanel;

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

        public async Task<int> GetUnshippedOrdersCount()
        {
            var unshippedOrders = await _context.Orders
                .Include(o => o.Status)
                .Where(o => o.Status.Name != "Order Closed" && o.Status.Name != "Order Shipped" && o.Status.Name != "Pending Appointment" && o.Status.Name != "Ongoing")
                .ToListAsync();

            return unshippedOrders.Count;
        }

        public async Task<List<OrderWithDetailsViewModel>> GetAllCustomerOrdersAsync(string customerId)
        {
            List<OrderWithDetailsViewModel> ordersWithDetails = new List<OrderWithDetailsViewModel>();
            List<Order> orders = await _context.Orders.Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.Appointment)
                .Include(o => o.Status)
                .Where(o => o.Customer.Id == customerId)
                .ToListAsync();
            

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

        public async Task<List<Order>> CheckAndConvertOrdersStatusAsync()
        {
            var statusCheckers = await _context.StatusCheckers.ToListAsync();

            if(statusCheckers == null || statusCheckers.Count() == 0)
            {
                var statusChecker = new StatusChecker
                {
                    Date = DateTime.UtcNow.AddDays(-1),
                };               

                await _context.StatusCheckers.AddAsync(statusChecker);
                await _context.SaveChangesAsync();

                statusCheckers.Add(statusChecker);
            }

            var checker = statusCheckers.First();

            if (checker.Date.AddMinutes(5) < DateTime.UtcNow)
            {

                var ordersCreated = await _context.Orders.Include(o => o.Status).Where(o => o.Status.Name == "Order Created").ToListAsync();
                var orderShipped = await _context.Orders.Include(o => o.Status).Where(o => o.Status.Name == "Order Shipped").ToListAsync();


                foreach(var order in ordersCreated)
                {
                    if(DateTime.UtcNow > order.OrderDate.AddMinutes(5))
                    {
                        order.Status = await GetOrderStatusByNameAsync("Order Shipped");
                        _context.Orders.Update(order);
                    }
                }

                foreach (var order in orderShipped)
                {
                    if (DateTime.UtcNow > order.OrderDate.AddMinutes(10))
                    {
                        order.Status = await GetOrderStatusByNameAsync("Order Closed");
                        _context.Orders.Update(order);
                    }                                      
                }

                checker.Date = DateTime.UtcNow;
                _context.StatusCheckers.Update(checker);
                await _context.SaveChangesAsync();

                return orderShipped;
            }

            return null;
        }

        public async Task<List<Order>> GetAllOrdersWithAppointmentsAsync()
        {
            return await _context.Orders.Include(o => o.Appointment).Include(o => o.Status).Include(o => o.Store).Where(o => o.Appointment != null).ToListAsync();
        }


        public async Task<bool> CheckIfCanReviewAsync(User user, Product product)
        {
            var userOrders = await _context.Orders.Include(o => o.Status).Where(o => o.Customer.Id == user.Id && o.Status.Name == "Order Closed" || o.Customer.Id == user.Id && o.Status.Name == "Appointment Done").ToListAsync();

            if(userOrders != null)
            {
                foreach(var order in userOrders)
                {
                    var orderDetails = await _context.OrderDetails.Where(od => od.Order.Id == order.Id && od.Product.Id == product.Id).FirstOrDefaultAsync();

                    if(orderDetails != null)
                    {
                        return true;                        
                    }                  
                }
            }

            return false;
        }


        public async Task<List<OrderChartViewModel>> GetUnshippedOrdersChartAsync(int month)
        {
            List<OrderChartViewModel> list = new List<OrderChartViewModel>();
            
            string[] status = new string[4] { "Order Created", "Appointment Created", "Order Shipped", "Order Closed" };

            string[] color = new string[4] { "#990000", "#FFA500", "#9C3BB0", "#9EB23B" };
            int id = 0;
            
            foreach (string statusItem in status)
            {
                var unshippedOrders = await _context.Orders
                    .Include(o => o.Status)
                    .Where(o => o.Status.Name == statusItem && o.OrderDate.Month == month && o.OrderDate.Year == DateTime.UtcNow.Year)
                    .ToListAsync();

                var newStatusItem = "";

                if (statusItem != "Pending Appointment" || statusItem != "Ongoing")
                {
                    if (statusItem == "Appointment Done") newStatusItem = "Order Closed";
                    else if (statusItem == "Appointment Created") newStatusItem = "Appoint. Created";
                    else newStatusItem = statusItem;

                    list.Add(new OrderChartViewModel
                    {
                        Status = newStatusItem,
                        Quantity = unshippedOrders.Count(),
                        Color = color[id]
                    });
                    id++;
                }
 
            }

            return list;
        }

        public async Task<List<ChartSalesViewModel>> GetMonthlySales(int month)
        {
            DateTime requestedDate = new DateTime(DateTime.UtcNow.Year, month, DateTime.UtcNow.Day);
            List<ChartSalesViewModel> list = new List<ChartSalesViewModel>();

            double value;
            CultureInfo ci = new CultureInfo("en-Us");
            int days = DateTime.DaysInMonth(DateTime.UtcNow.Year, month);

            for (int day = 1; day <= days; day++)
            {
                value = (double)await _context.Orders.Where(i => i.OrderDate.Day == day && i.OrderDate.Month == requestedDate.Month).SumAsync(i => i.TotalPrice);

                list.Add(new ChartSalesViewModel
                {
                    Month = requestedDate.ToString("MMMM", ci).ToUpper(),
                    Day = day,
                    Sales = value
                });
            }

            return list;
        }

        public async Task<List<ChartSalesViewModel>> GetYearSalesByMonthAsync(int year)
        {
            DateTime requestedYearDate = new DateTime(year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);

            List<ChartSalesViewModel> list = new List<ChartSalesViewModel>();

            double value;
            CultureInfo ci = new CultureInfo("en-Us");


            for (int month = 1; month <= 12; month++)
            {
                value = (double)await _context.Orders.Where(i => i.OrderDate.Month == month && i.OrderDate.Year == requestedYearDate.Year).SumAsync(i => i.TotalPrice);

                list.Add(new ChartSalesViewModel
                {
                    Year = year.ToString(),
                    Month = new DateTime(year, month, DateTime.UtcNow.Day).ToString("MMMM", ci),
                    Sales = value,
                });
            }

            return list;
        }
    }
}
