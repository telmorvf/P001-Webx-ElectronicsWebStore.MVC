using Microsoft.AspNetCore.Mvc.Rendering;
﻿using Microsoft.EntityFrameworkCore;
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

        public async Task CheckAndConvertOrdersStatusAsync()
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
            }
        }

        public async Task<List<Order>> GetAllOrdersWithAppointmentsAsync()
        {
            return await _context.Orders.Include(o => o.Appointment).Include(o => o.Status).Include(o => o.Store).Where(o => o.Appointment != null).ToListAsync();
        }
    }
}
