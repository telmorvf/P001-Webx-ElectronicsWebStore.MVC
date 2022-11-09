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
    public class EmployeeController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly INotyfService _toastNotification;
        private readonly IConverterHelper _converterHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;

        public EmployeeController(IUserHelper userHelper,
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
            //TO DO:
            // Alinhar headers text da tabela do syncfusion ao centro

            IEnumerable<User> employees;

            //vai buscar os empregados da empresa (Admins/technicians/product managers)
            if (isActive)
            {
                employees = await _userHelper.GetAllActiveEmployeesAsync();
            }
            else
            {
                employees = await _userHelper.GetAllEmployeesAsync();
            }

            ViewBag.IsActive = isActive;
            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(User);

            return View(employees);
        }    
        
        public async Task<IActionResult> Update(string id)
        {
            if(id == null)
            {
                _toastNotification.Error("User Id was not found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if(user == null)
            {
                _toastNotification.Error("User could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var model = await _converterHelper.ToEditEmployeeViewModelAsync(user);


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditEmployeeViewModel model)
        {                      
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.Id);

                if(user == null)
                {
                    _toastNotification.Error("Error, the user was not found");
                    return RedirectToAction(nameof(ViewAll));
                }

                Guid imageId = user.ImageId;

                if (model.PictureFile != null && model.PictureFile.Length > 0)
                {                                        
                    using var image = Image.Load(model.PictureFile.OpenReadStream());
                    image.Mutate(img => img.Resize(512, 0));

                    using (MemoryStream m = new MemoryStream())
                    {
                        image.SaveAsJpeg(m);
                        byte[] imageBytes = m.ToArray();
                        imageId = await _blobHelper.UploadBlobAsync(imageBytes, "users");
                    } 
                    
                    model.ImageId = imageId;
                }

                user.ImageId = imageId;

                if(model.RoleId != null && model.RoleId != "0")
                {
                    var role = await _userHelper.GetRoleByIdAsync(model.RoleId);                   
                    bool alreadyInRole = false;

                    if(role == null)
                    {
                        _toastNotification.Error("There was a problem updating the user role. Please try again");
                        return RedirectToAction(nameof(ViewAll));
                    }

                    if(await _userHelper.CheckUserInRoleAsync(user,role.Name))
                    {
                        alreadyInRole = true;
                        _toastNotification.Information($"User was already as {role.Name} role",10);
                    }                    

                    if (!alreadyInRole)
                    {
                        var response = await _userHelper.RemoveFromCurrentRoleAsync(user,model.CurrentRole);

                        if (response.Succeeded)
                        {
                           var result = await _userHelper.AddUserToRoleAsync(user, role.Name);

                           if (!result.Succeeded)
                           {
                               _toastNotification.Error("There was a problem updating the user role. Please try again");
                               return RedirectToAction(nameof(ViewAll));
                           }

                        }
                        else
                        {
                            _toastNotification.Error("There was a problem removing the user from the current role. Please try again");
                            return RedirectToAction(nameof(ViewAll));
                        }
                    }                                    

                    model.CurrentRole = role.Name;
                }

                bool changedOwnEmail = false;


                if(user.Email != model.Email)
                {
                    if (this.User.Identity.Name == user.Email)
                    {
                        changedOwnEmail = true;
                    }
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    await _userHelper.ConfirmEmailAsync(user, token);                   
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;                
                user.PhoneNumber = model.PhoneNumber;
                user.NIF = model.NIF;
                user.Address = model.Address;
                user.Active = model.Active;
                model.Roles = _userHelper.GetEmployeesComboRoles();
                
                try
                {
                   await _userHelper.UpdateUserAsync(user);
                   if (changedOwnEmail)
                   {
                        _toastNotification.Success("Your data has been updated with success!");
                        _toastNotification.Information("You have been loged out due to email change",10);
                        await _userHelper.LogoutAsync();
                        return RedirectToAction("Index", "Home");
                   }
                   _toastNotification.Success("Employee updated with success!");                    
                   return View(model);
                }
                catch (Exception ex)
                {
                    _toastNotification.Error($"There was a problem updating the employee! {ex.InnerException.Message} ");
                    return View(model);
                }

            }

            model.Roles = _userHelper.GetEmployeesComboRoles();

            return View(model);
        }

        public IActionResult Create()
        {

            var model = new CreateEmployeeViewModel();
            model.Roles = _userHelper.GetEmployeesComboRoles();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            model.Roles = _userHelper.GetEmployeesComboRoles();

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

                        if (model.PictureFile != null && model.PictureFile.Length > 0)
                        {
                            Guid imageId = Guid.Empty;

                            using var image = Image.Load(model.PictureFile.OpenReadStream());
                            image.Mutate(img => img.Resize(512, 0));

                            using (MemoryStream m = new MemoryStream())
                            {
                                image.SaveAsJpeg(m);
                                byte[] imageBytes = m.ToArray();
                                imageId = await _blobHelper.UploadBlobAsync(imageBytes, "users");
                            }

                            user.ImageId = imageId;
                            model.ImageId = imageId;
                        }

                        var result = await _userHelper.AddUserAsync(user, "DefaultPassword123");

                        if (!result.Succeeded)
                        {
                            _toastNotification.Error("There was a creating the user. Please try again");
                            return View(model);
                        }

                        if (model.RoleId != null && model.RoleId != "0")
                        {
                            var role = await _userHelper.GetRoleByIdAsync(model.RoleId);

                            if (role == null)
                            {
                                _toastNotification.Error("There was a problem selecting the desired role. Please try again");
                                return RedirectToAction("Index", "AdminPanel");
                            }

                            result = await _userHelper.AddUserToRoleAsync(user, role.Name);

                            if (!result.Succeeded)
                            {
                                _toastNotification.Error("There was a problem associatinh the user to the role. Please try again");
                                return RedirectToAction("Index", "AdminPanel");
                            }
                        }
                        else
                        {
                            _toastNotification.Error("You must select a role for the new user!");                           
                            return View(model);
                        }                      

                        string userToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                        string tokenLink = Url.Action("ConfirmEmail", "Account", new
                        {
                            userId = user.Id,
                            token = userToken
                        }, protocol: HttpContext.Request.Scheme);
                        var returnLink = Url.Action("Index", "Home", null, protocol: HttpContext.Request.Scheme);

                        Response response = await _mailHelper.SendEmployeeConfirmationEmail(user.Email, tokenLink, user, returnLink);

                        if (response.IsSuccess)
                        {
                            _toastNotification.Success($"An email has been sent to {user.Email}, please check your email and follow the instructions.",10);
                            model = new CreateEmployeeViewModel();
                            model.Roles = _userHelper.GetEmployeesComboRoles();
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
                _toastNotification.Error("There was a problem resolving the employee id. Try again later");
                return RedirectToAction("ViewAll", "Employee", new { isActive = true });
            }

            var user = await _userHelper.GetUserByIdAsync(id);

            if (user == null)
            {
                _toastNotification.Error("There was a problem getting the user data. Try again later");
                return RedirectToAction("ViewAll", "Employee", new { isActive = true });
            }

            user.Active = true;

            var wasSaved = await _userHelper.UpdateUserAsync(user);

            if (wasSaved.Succeeded)
            {
                _toastNotification.Success($"{user.FullName} account was reactivated with success!");
                return RedirectToAction("Update", "Employee", new { id = user.Id });
            }

            _toastNotification.Error("There was a problem saving the user data. Try again later");
            return RedirectToAction("ViewAll", "Employee", new { isActive = true });
        }

        [HttpPost]
        [Route("Employee/EmployeeDetails")]
        public async Task<JsonResult> EmployeeDetails(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return null;
            }

            var user = await _userHelper.GetUserByIdAsync(Id);

            if(user == null)
            {
                return null;
            }

            var model = await _converterHelper.ToEditEmployeeViewModelAsync(user);

            if(model == null)
            {
                return null;
            }

            var json = Json(model);
            return json;
        }

        [HttpPost]
        [Route("Employee/Delete")]
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
        [Route("Employee/ToastNotification")]
        public JsonResult ToastNotification(string message, string type)
        {
            bool result = false;

            if(type == "success")
            {
                _toastNotification.Success(message,5);
                result = true;
            }
            
            if(type== "error")
            {                
                _toastNotification.Error(message,5);
                result = true;
            }

            if (type == "warning")
            {
                _toastNotification.Warning(message,5);
                result = true;
            }

            if (type == "Information")
            {
                _toastNotification.Information(message,5);
                result = true;
            }            

            return Json(result);
        }


    }
}
