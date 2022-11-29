using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;

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
        private readonly IStoreRepository _storeRepository;

        public AppointmentController(IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserHelper userHelper,
            IAppointmentRepository appointmentRepository,
            INotyfService toastNotification,
            IBrandRepository brandRepository,
            IStoreRepository storeRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userHelper = userHelper;
            _appointmentRepository = appointmentRepository;
            _toastNotification = toastNotification;
            _brandRepository = brandRepository;
            _storeRepository = storeRepository;
        }


        public async Task<IActionResult> Index(int? storeId)
        {
            AppointmentViewModel model = new AppointmentViewModel();
            model.PhysicalStoresCombo = _storeRepository.GetComboAppointmentsStores();
            var ordersWithappointments = await _orderRepository.GetAllOrdersWithAppointmentsAsync();
            var events = "";

            if (storeId == null || storeId == 0)
            {                
                model.StoreId = 0;                

                model.AppointmentsCreatedTotals = ordersWithappointments.Where(o => o.Status.Name == "Appointment Created").Count();
                model.AppointmentsDoneTotals = ordersWithappointments.Where(o => o.Status.Name == "Appointment Done").Count();
                model.OngoingAppointmentsTotals = ordersWithappointments.Where(o => o.Status.Name == "Ongoing").Count();

                events = await _appointmentRepository.GetAllEventsAsync();
            }
            else
            {               
                model.StoreId = storeId.Value;

                model.AppointmentsCreatedTotals = ordersWithappointments.Where(o => o.Status.Name == "Appointment Created" && o.Store.Id == storeId.Value).Count();
                model.AppointmentsDoneTotals = ordersWithappointments.Where(o => o.Status.Name == "Appointment Done" && o.Store.Id == storeId.Value).Count();
                model.OngoingAppointmentsTotals = ordersWithappointments.Where(o => o.Status.Name == "Ongoing" && o.Store.Id == storeId.Value).Count();

                events = await _appointmentRepository.GetSpecificStoreEvents(storeId.Value);
            }           

            
            ViewData["Events"] = events;

            return View(model);
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

        public async Task<bool> EventResize(int eventId, string startTime, string endTime)
        {
            if (eventId == 0 || string.IsNullOrEmpty(startTime) || string.IsNullOrEmpty(endTime))
            {
                return false;
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(eventId);

            if (appointment == null)
            {
                return false;
            }

            var startDateTime = Convert.ToDateTime(startTime).ToUniversalTime();
            var endDateTime = Convert.ToDateTime(endTime).ToUniversalTime();

            appointment.EndHour = endDateTime;
            appointment.BegginingHour = startDateTime;
            appointment.AppointmentDate = startDateTime;
            
            try
            {
                await _appointmentRepository.UpdateAsync(appointment);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeclareAsAttended(int? id,int? appointmentId)
        {
            bool result = false;

            if(id == 0 || id == null || appointmentId == 0 || appointmentId == null)
            {
                return Json(result);
            }

            var order = await _orderRepository.GetOrderByIdAsync(id.Value);

            if(order == null)
            {
                return Json(result);
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId.Value);

            if(appointment == null)
            {
                return Json(result);
            }            

            var onGoingStatus = await _orderRepository.GetOrderStatusByNameAsync("Ongoing");

            if(onGoingStatus == null)
            {
                return Json(result);
            }

            appointment.HasAttended = true;
            order.Status = onGoingStatus;

            try
            {
                await _orderRepository.UpdateAsync(order);
                await _appointmentRepository.UpdateAsync(appointment);
                result = true;
                return Json(result);
            }
            catch (Exception)
            {
                return Json(result);
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeclareAsDone(int? id, int? appointmentId)
        {
            bool result = false;

            if (id == 0 || id == null || appointmentId == 0 || appointmentId == null)
            {
                return Json(result);
            }

            var order = await _orderRepository.GetOrderByIdAsync(id.Value);

            if (order == null)
            {
                return Json(result);
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId.Value);

            if (appointment == null)
            {
                return Json(result);
            }

            var appointmentDoneStatus = await _orderRepository.GetOrderStatusByNameAsync("Appointment Done");

            if (appointmentDoneStatus == null)
            {
                return Json(result);
            }

            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if(user == null)
            {
                return Json(result);
            }

            appointment.WorkerID = user;
            order.Status = appointmentDoneStatus;

            try
            {
                await _orderRepository.UpdateAsync(order);
                await _appointmentRepository.UpdateAsync(appointment);
                result = true;
                return Json(result);
            }
            catch (Exception)
            {
                return Json(result);
            }
        }

    }
}
