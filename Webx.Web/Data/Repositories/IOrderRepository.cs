using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;
using Webx.Web.Models.AdminPanel;

namespace Webx.Web.Data.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {

        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDetail>> GetAllOrderDetailsByOrderIdAsync(int id);
        Task<List<Order>> GetCustomerRecentOrdersAsync(User user, DateTime date);
        Task<Response> CreateOrderDetailsAsync(List<OrderDetail> orderDetails);
        Task<Status> GetOrderStatusByNameAsync(string orderStatusName);
        Task<List<OrderDetail>> GetOrderDetailsAsync(int id);
        Task AddOrderAsync(OrderViewModel orderVm,int storeId);
        Task<Order> GetCompleteOrderByIdAsync(int orderId);        
        Task<List<OrderWithDetailsViewModel>> GetAllCustomerOrdersAsync(string customerId);
        Task<List<Order>> CheckAndConvertOrdersStatusAsync();
        Task<List<Order>> GetAllOrdersWithAppointmentsAsync();
        Task<bool> CheckIfCanReviewAsync(User user, Product product);
        Task<int> GetUnshippedOrdersCount();
        Task<List<OrderChartViewModel>> GetUnshippedOrdersChartAsync(int month);
        Task<List<ChartSalesViewModel>> GetMonthlySales(int month);
        Task<List<ChartSalesViewModel>> GetYearSalesByMonthAsync(int year);

    }
}
