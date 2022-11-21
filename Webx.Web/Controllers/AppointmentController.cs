using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;

namespace Webx.Web.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserHelper _userHelper;

        public AppointmentController(IOrderRepository orderRepository,IProductRepository productRepository,IUserHelper userHelper)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userHelper = userHelper;
        }


        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> MakeAppointment(string orders)
        {
            var model = await _productRepository.GetInitialShopViewModelAsync();
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;

            if (!string.IsNullOrEmpty(orders))
            {
                string[] ordersArray = orders.Split(',');

                List<Order> ordersList = new List<Order>();

                foreach (string item in ordersArray)
                {
                    int orderId = int.Parse(item);
                    var order = await _orderRepository.GetCompleteOrderByIdAsync(orderId);
                    ordersList.Add(order);
                }
            }






            return View(model);
        }
    }
}
