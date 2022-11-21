using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<List<Order>> GetCustomerRecentOrdersAsync(User user, DateTime date);
        Task<Response> CreateOrderDetailsAsync(List<OrderDetail> orderDetails);

        Task<OrderStatus> GetOrderStatusByNameAsync(string orderStatusName);
        Task<List<OrderDetail>> GetOrderDetailsAsync(int id);
        Task AddOrderAsync(OrderViewModel orderVm,int storeId);
        Task<Order> GetCompleteOrderByIdAsync(int orderId);
    }
}
