using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Base;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Webx.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly INotyfService _toastNotification;
        private readonly IConverterHelper _converterHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;

        public CustomerController(IUserHelper userHelper,
            INotyfService toastNotification,
            IConverterHelper converterHelper,
            IBlobHelper blobHelper,
            IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _toastNotification = toastNotification;
            _converterHelper = converterHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
        }

        public async Task<IActionResult> ViewAll(bool isActive)
        {

            IEnumerable<User> customers;

            if (isActive)
            {
                customers = await _userHelper.GetAllActiveCustomersAsync();
            }
            else
            {
                customers = await _userHelper.GetAllCustomersUsersAsync();
            }

            ViewBag.IsActive = isActive;
            ViewBag.Type = typeof(User);

            return View(customers);
        }

        public async Task<IActionResult> Update(string id)
        {
            if (id == null)
            {
                _toastNotification.Error("User Id was not found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                _toastNotification.Error("User could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var model = _converterHelper.ToEditCustomerViewModel(user);


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditCustomerViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.Id);

                if (user == null)
                {
                    _toastNotification.Error("Error, the user was not found");
                    return RedirectToAction(nameof(ViewAll));
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.NIF = model.NIF;
                user.Address = model.Address;

                try
                {
                    await _userHelper.UpdateUserAsync(user);
                    
                    _toastNotification.Success("Customer updated with success!");
                    return View(model);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        _toastNotification.Error($"The nif  {user.NIF}  already exists!");
                    }
                    else
                    {
                        _toastNotification.Error($"There was a problem updating the employee!");
                    }
                    return View(model);
                }

            }
            return View(model);
        }

        public IActionResult Create()
        {

            var model = new CreateCustomerViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    if (model.NIF != null)
                    {
                        user = await _userHelper.GetUserByNIFAsync(model.NIF);
                    }

                    if (user == null)
                    {
                        user = new User
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            PhoneNumber = model.PhoneNumber,
                            Address = model.Address,
                            NIF = model.NIF,
                            Active = true,
                            Email = model.Email,
                            UserName = model.Email,
                        };

                        var result = await _userHelper.AddUserAsync(user, "DefaultPassword123");

                        if (!result.Succeeded)
                        {
                            _toastNotification.Error("There was an error creating the user. Please try again");
                            return View(model);
                        }

                        var role = await _userHelper.GetRoleByNameAsync("Customer");

                        if (role == null)
                        {
                            _toastNotification.Error("There was a problem selecting the desired role. Please try again");
                            return RedirectToAction("Index", "AdminPanel");
                        }

                        result = await _userHelper.AddUserToRoleAsync(user, role.Name);

                        if (!result.Succeeded)
                        {
                            _toastNotification.Error("There was a problem associating the user to the role. Please try again");
                            return RedirectToAction("Index", "AdminPanel");
                        }
                       
                        string userToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                        string tokenLink = Url.Action("ConfirmEmail", "Account", new
                        {
                            userId = user.Id,
                            token = userToken
                        }, protocol: HttpContext.Request.Scheme);
                        var returnLink = Url.Action("Index", "Home", null, protocol: HttpContext.Request.Scheme);

                        Response response = await _mailHelper.SendConfirmationEmail(user.Email, tokenLink, user, returnLink);

                        if (response.IsSuccess)
                        {
                            _toastNotification.Success($"An email has been sent to {user.Email}, please check your email and follow the instructions.", 10);
                            model = new CreateCustomerViewModel();
                            return View(model);
                        }

                    }

                    _toastNotification.Warning("A user with the typed nif already exists!");
                    return View(model);

                }

                _toastNotification.Warning("A user with the typed email already exists!");
                return View(model);
            }

            return View(model);
        }
        public async Task<IActionResult> Reactivate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _toastNotification.Error("There was a problem resolving the customer id. Try again later");
                return RedirectToAction("ViewAll", "Customer", new { isActive = true });
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                _toastNotification.Error("There was a problem getting the user data. Try again later");
                return RedirectToAction("ViewAll", "Customer", new { isActive = true });
            }

            user.Active = true;

            var wasSaved = await _userHelper.UpdateUserAsync(user);

            if (wasSaved.Succeeded)
            {
                _toastNotification.Success($"{user.FullName} account was reactivated with success!");
                return RedirectToAction("Update", "Customer", new { id = user.Id });
            }

            _toastNotification.Error("There was a problem saving the user data. Try again later");
            return RedirectToAction("ViewAll", "Customer", new { isActive = true });
        }

        [HttpPost]
        [Route("Customer/CustomerDetails")]
        public async Task<JsonResult> CustomerDetails(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return null;
            }

            var user = await _userHelper.GetUserByIdAsync(Id);

            if (user == null)
            {
                return null;
            }

            var model = _converterHelper.ToEditCustomerViewModel(user);

            if (model == null)
            {
                return null;
            }

            var json = Json(model);
            return json;
        }


        [HttpPost]
        [Route("Customer/Delete")]
        public async Task<JsonResult> Delete(string Id)
        {
            bool result = false;

            if (string.IsNullOrEmpty(Id))
            {
                return Json(result);
            }

            var user = await _userHelper.GetUserByIdAsync(Id);

            if (user == null)
            {
                return Json(result);
            }

            user.Active = false;

            var wasSaved = await _userHelper.UpdateUserAsync(user);

            if (wasSaved.Succeeded)
            {
                result = true;
            }

            return Json(result);
        }

        [HttpPost]
        [Route("Customer/ToastNotification")]
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
