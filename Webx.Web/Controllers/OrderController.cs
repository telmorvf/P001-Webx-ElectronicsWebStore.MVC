using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;


namespace Webx.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly INotyfService _toastNotification;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IStatusRepository _statusRepository;

        public OrderController(IOrderRepository orderRepository,
                               INotyfService toastNotification,
                               IUserHelper userHelper,
                               IConverterHelper converterHelper,                             
                               IStatusRepository statusRepository
            )
        {
            _orderRepository = orderRepository;
            _toastNotification = toastNotification;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _statusRepository = statusRepository;
        }

        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Order> orders;

            orders = await _orderRepository.GetAllOrdersAsync();

            ViewBag.Type = typeof(Order);

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            //TODO add "badge rounded pill info" ?
            if (id==null)
            {
                _toastNotification.Error("Order Id was not found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var order = await _orderRepository.GetOrderByIdAsync(id.Value);
            var orderDetails = await _orderRepository.GetAllOrderDetailsByOrderIdAsync(id.Value);

            if (order == null)
            {
                _toastNotification.Error("Order could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var model = _converterHelper.ToOrderViewModel(order);
            
            model.OrderDetails = orderDetails;
            model.StatusId = model.Status.Id.ToString();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Update(OrderViewModel model)
        {
            
            
                var order = await _orderRepository.GetOrderByIdAsync(model.Id);

                if (order == null)
                {
                    _toastNotification.Error("Error, the order was not found");
                    return RedirectToAction(nameof(ViewAll));
                }

                if (model.StatusId != null && model.StatusId != "0")
                {
                    var status = await _statusRepository.GetStatusByIdAsync(model.StatusId);
                    bool sameStatus = false;

                    if (status == null)
                    {
                        _toastNotification.Error("There was a problem updating the order status. Please try again");
                        return RedirectToAction(nameof(ViewAll));
                    }

                    if (order.Status == status)
                    {
                        sameStatus = true;
                        _toastNotification.Information($"Order status was already as {status.Name}", 10);
                        return RedirectToAction(nameof(ViewAll));
                    }

                    if (!sameStatus)
                    {
                        order.Status = status;
                    }
                }

                try
                {
                    await _orderRepository.UpdateAsync(order);
                    _toastNotification.Success("Order status updated!");
                    return RedirectToAction("ViewAll", "Order");
                }
                catch (Exception ex)
                {
                    _toastNotification.Error($"There was a problem updating the order status! {ex.InnerException.Message} ");
                    return RedirectToAction("ViewAll", "Order");
                };

            
        }

        [HttpPost]
        [Route("Order/ToastNotification")]
        public JsonResult ToastNotification(string message, string type)
        {
            bool result = false;

            if (type == "success")
            {
                _toastNotification.Success(message, 5);
                result = true;
            }

            if (type == "error")
            {
                _toastNotification.Error(message, 5);
                result = true;
            }

            if (type == "warning")
            {
                _toastNotification.Warning(message, 5);
                result = true;
            }

            if (type == "Information")
            {
                _toastNotification.Information(message, 5);
                result = true;
            }

            return Json(result);
        }
    }
}
