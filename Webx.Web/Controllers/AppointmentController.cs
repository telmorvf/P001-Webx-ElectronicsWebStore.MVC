using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
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
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly INotyfService _toastNotification;
        private readonly IBrandRepository _brandRepository;

        public AppointmentController(IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserHelper userHelper,
            IAppointmentRepository appointmentRepository,
            INotyfService toastNotification,
            IBrandRepository brandRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userHelper = userHelper;
            _appointmentRepository = appointmentRepository;
            _toastNotification = toastNotification;
            _brandRepository = brandRepository;
        }


        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> MakeAppointment(int orderId)
        {
            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;

            var order = await _orderRepository.GetCompleteOrderByIdAsync(orderId);

            if(order == null)
            {
                return NotFound();
            }

            //se tentarem proceder a marcação de order Id que não pertence a conta de origem
            if(order.Customer.Id != user.Id)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            model.OrderToSchedule = order;
            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();

            var events = await _appointmentRepository.GetAllEventsCustomerCanSeeAsync(order.Store.Id);
            ViewData["Events"] = events;

            return View(model);
        }

        public async Task<IActionResult> SaveAppointment(string startDate, string endDate,int orderId,string comment)
        {

            if(string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return NotFound();
            }

            var apointmentStartDate = Convert.ToDateTime(startDate);
            var apointmentEndDate = Convert.ToDateTime(endDate);
            var order = await _orderRepository.GetCompleteOrderByIdAsync(orderId);

            if(order == null)
            {
                return NotFound();
            }

            var appointment = new Appointment
            {
                AppointmentDate = apointmentStartDate.Date,
                BegginingHour = apointmentStartDate,
                EndHour = apointmentEndDate,
                Comments = comment,
            };

            try
            {
                await _appointmentRepository.CreateAsync(appointment);
                order.Appointment = appointment;
                order.Status = await _orderRepository.GetOrderStatusByNameAsync("Appointment Created");
                await _orderRepository.UpdateAsync(order);
                _toastNotification.Success("Appointment created with success!");
            }
            catch (Exception ex)
            {
                _toastNotification.Error("There was a problem creating the appointment."+ex.Message);                
            }


            return RedirectToAction("ViewUser", "Account");
        }


    }
}
