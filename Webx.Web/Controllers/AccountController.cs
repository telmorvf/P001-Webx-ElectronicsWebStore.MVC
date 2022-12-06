using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using AspNetCoreHero.ToastNotification.Abstractions;
using Webx.Web.Data.Repositories;
using System.Collections.Generic;
using X.PagedList;

namespace Webx.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBlobHelper _blobHelper;
        private readonly INotyfService _toastNotification;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IBrandRepository _brandRepository;

        public AccountController(IUserHelper userHelper,IMailHelper mailHelper, ICategoryRepository categoryRepository,IBlobHelper blobHelper
            , INotyfService toastNotification, IProductRepository productRepository,IOrderRepository orderRepository,IBrandRepository brandRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _categoryRepository = categoryRepository;            
            _blobHelper = blobHelper;
            _toastNotification = toastNotification;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _brandRepository = brandRepository;
        }

        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel();

            if (returnUrl != null)
            {
                model.ReturnUrl = returnUrl;
                ViewData["ReturnUrl"] = returnUrl;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {

                var result = await _userHelper.LoginAsync(model);

                if (result.Succeeded)
                {
                    if (this.Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(this.Request.Query["ReturnUrl"].First());
                    }

                    var user = await _userHelper.GetUserByEmailAsync(model.UserName);

                    if (user != null)
                    {

                        if (model.ReturnUrl != null)
                        {
                            return Redirect(model.ReturnUrl);
                                
                        }

                        return this.RedirectToAction("Index", "Home");

                        //var isCustomerRole = await _userHelper.CheckUserInRoleAsync(user, "Customer");

                        //if (isCustomerRole)
                        //{
                        //    return this.RedirectToAction("Index", "Home");
                        //}
                        //else
                        //{
                        //    return RedirectToAction("Index", "DashboardPanel");
                        //}
                    }

                    ModelState.AddModelError(string.Empty, "Failed to Login");
                }
            }

            ModelState.AddModelError(string.Empty, "Failed to login");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {

            var model = new RegisterViewModel();
            model.ReturnUrl = returnUrl;
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.UserName);

                if (user == null)
                {
                    if(model.NIF != null)
                    {
                        user = await _userHelper.GetUserByNIFAsync(model.NIF);
                    }                    

                    if(user == null)
                    {
                        user = new User
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.UserName,
                            UserName = model.UserName,
                            Address = model.Address,
                            PhoneNumber = model.PhoneNumber,
                            NIF = model.NIF,
                            Active = true
                        };

                        var result = await _userHelper.AddUserAsync(user, "DefaultPassword123");

                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, "The user couldn't be created. Probably Email is alredy registered.");
                            return View(model);
                        }

                        result = await _userHelper.AddUserToRoleAsync(user, "Customer");

                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError(string.Empty, "The user couldn't be created, failed to assign as customer");
                            return View(model);
                        }

                        string userToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                        string tokenLink = Url.Action("ConfirmEmail", "Account", new
                        {
                            userId = user.Id,
                            token = userToken,
                            returnUrl = model.ReturnUrl,                            
                        }, protocol: HttpContext.Request.Scheme);

                                        
                        string returnLink = Url.Action("Index", "Home", null, protocol: HttpContext.Request.Scheme);
                     

                        Response response = await _mailHelper.SendConfirmationEmail(model.UserName, tokenLink, user,returnLink);

                        if (response.IsSuccess)
                        {
                            ViewBag.Message = $"An email has been sent to {user.Email}, please check your email and follow the instructions.";
                            return View(model);
                        }
                    }
                    ModelState.AddModelError(string.Empty, "The NIF you're trying to introduce is already being used.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "The user couldn't be created, probably Email is alredy registered.");
                return View(model);
                
            }                        

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return NotFound();
            }

            var model = new AddUserPasswordViewModel
            {
                UserId = userId,
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(AddUserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByIdAsync(model.UserId);
                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, "DefaultPassword123", model.Password);
                    if (result.Succeeded)
                    {
                        if(model.ReturnUrl != null)
                        {
                            var loginAttempt = await _userHelper.FirstLoginAsync(user);
                            if (loginAttempt.Succeeded)
                            {
                                return Redirect(model.ReturnUrl);
                            }
                            else
                            {
                                ViewBag.Failed = "Unfurtunaly theere was a problem redirecting you to the checkout page.";
                                return View(model);
                            }
                            
                        }                       

                        ViewBag.Success = "You can now login into the system.";
                        return View(model);
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "User not found");
                }
            }
            return View(model);
        }

        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The Email does not correspond to a registered email.");
                    return View(model);
                }

                var userToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var link = Url.Action("ResetPassword", "Account", new { token = userToken, userId = user.Id }, protocol: HttpContext.Request.Scheme);
                var returnLink = Url.Action("Index", "Home",null, protocol: HttpContext.Request.Scheme);

                Response response = await _mailHelper.SendResetPasswordEmail(model.Email, link, user,returnLink);

                if (response.IsSuccess)
                {
                    ViewBag.Message = "The instructions to recover your password have been sent to the email address.";
                }

                return View();
            }

            return View(model);
        }


        public async Task<IActionResult> ResetPassword(string token, string userId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ResetPasswordViewModel
            {
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.UserName);
                if (user != null)
                {
                    var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        if (user.EmailConfirmed == false)
                        {
                            var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                            await _userHelper.ConfirmEmailAsync(user, token);
                        }

                        ViewBag.Message = "Password Reset Successful, you can now Login with the new credentials.";
                        return View();
                    }


                    ViewBag.Message = "There was an error while resseting your password.";
                    return View(model);
                }

                ViewBag.Message = "User was not found";
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirect = Url.Action("ExternalLoginCallback", "Account", new { returnUrl = returnUrl });
            var properties = _userHelper.ConfigureExternalAuthenticationProperties(provider, redirect);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, "Error from external provider");
                return View("Login");
            }

            var info = await _userHelper.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userHelper.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                await _userHelper.UpdateExternalAuthenticationTokensAsync(info);
                if (returnUrl != null)
                {
                    return LocalRedirect(returnUrl);
                }
                else return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["ProvierDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginViewModel { Email = email });
            }
        }

#nullable enable
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string? returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var info = await _userHelper.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("Error");
                }
                var user = new User
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Active = true
                };

                var result = await _userHelper.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await _userHelper.AddUserToRoleAsync(user, "Customer");

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "The user couldn't be created, failed to assign as customer");
                        return View(model);
                    }

                    var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    await _userHelper.ConfirmEmailAsync(user, token);
                    await _userHelper.UpdateUserAsync(user);

                    var loginResult = await _userHelper.AddLoginAsync(user, info);

                    if (loginResult.Succeeded)
                    {
                        await _userHelper.SignInAsync(user, isPersistent: false);
                        await _userHelper.UpdateExternalAuthenticationTokensAsync(info);
                        return LocalRedirect(returnUrl);
                    }
                }
                ModelState.AddModelError("Email", "User already exists");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }
#nullable disable

        [Authorize]
        public async Task<IActionResult> ViewUser(bool redirect = false)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }         

            var hasPassword = await _userHelper.HasPasswordAsync(user);

            var changeUserViewModel = new ChangeUserViewModel
            {
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                Address = user.Address,
                Email = user.Email,
                LastName = user.LastName,
                NIF = user.NIF,
                ImageId = user.ImageId,
                HasPassword = hasPassword,
            };

            var custOrders = await _orderRepository.GetAllCustomerOrdersAsync(user.Id);
            bool hasAppointmentToDo = false;
            
            foreach(var order in custOrders)
            {
                if(order.Order.Status.Name == "Pending Appointment")
                {
                    hasAppointmentToDo = true;
                    break;
                }
            }

            var model = new ShopViewModel();

           
           model = new ShopViewModel
           {
               UserViewModel = changeUserViewModel,
               Cart = await _productRepository.GetCurrentCartAsync(),
               CustomerOrders = custOrders,
               HasAppointmentToDo = hasAppointmentToDo,
               Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync(),
               WishList = await _productRepository.GetOrStartWishListAsync(),
               GoToWishList = redirect
           };
           

            ViewBag.JsonModel = JsonConvert.SerializeObject(model);
            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;         

            return View(model);
        }

        
        [HttpPost]
        [Route("Account/UpdateUser")]
        public async Task<JsonResult> UpdateUser(string email, long phoneNumber, string nif)
        {
            bool isValid = false;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return Json(isValid);
            }                
                
            user.Email = email;
            user.UserName = email;            
            user.PhoneNumber = phoneNumber.ToString();
            user.NIF = nif;           

            try
            {
                await _userHelper.UpdateUserAsync(user);

                isValid = true;
            }
            catch (Exception)
            {
                return Json(isValid);
            }

            return Json(isValid);
        }

        [HttpPost]
        [Route("Account/UpdateUserAddress")]
        public async Task<JsonResult> UpdateUserAddress(string address)
        {
            bool isValid = false;
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return Json(isValid);
            }
            
            user.Address = address;         

            try
            {
                await _userHelper.UpdateUserAsync(user);

                isValid = true;
            }
            catch (Exception)
            {
                return Json(isValid);
            }

            return Json(isValid);
        }

        [HttpPost]
        [Route("Account/ChangePassword")]
        public async Task<JsonResult> ChangePassword(string oldPassword, string newPassword, string repeatedPassword)
        {

            Response response;

            if (newPassword != repeatedPassword)
            {
                response = new Response
                {
                    IsSuccess = false,
                    Message = "New password is not equivalent to the repeated password."
                };

                return Json(response);
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return Json(new Response
                {
                    IsSuccess = false,
                    Message = "User is not valid",
                });
            }

            var isActualUser = await _userHelper.CheckPasswordAsync(user, oldPassword);

            if (isActualUser.Succeeded)
            {

                var result = await _userHelper.ChangePasswordAsync(user, oldPassword, newPassword);
                if (result.Succeeded)
                {
                    return Json(new Response { IsSuccess = true, Message = "Password was succefully changed!" });
                }
                else
                {
                    return Json(new Response
                    {
                        IsSuccess = false,
                        Message = "There was a problem changing the password.",
                    });
                }
            }

            return Json(new Response
            {
                IsSuccess = false,
                Message = "The old password is not correct.",
            });

        }

        [HttpPost]
        [Route("Account/GetProfilePicturePath")]
        public async Task<JsonResult> GetProfilePicturePath()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            var json = Json(user);
            return json;
        }

        [HttpPost]
        [Route("Account/ChangeProfilePic")]
        public async Task<IActionResult> ChangeProfilePic(IFormFile file)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if (user != null && file != null)
            {

                Guid imageId = user.ImageId;

                if (file != null && file.Length > 0)
                {
                    using var image = Image.Load(file.OpenReadStream());
                    image.Mutate(img => img.Resize(512, 0));

                    using (MemoryStream m = new MemoryStream())
                    {
                        image.SaveAsJpeg(m);
                        byte[] imageBytes = m.ToArray();
                        imageId = await _blobHelper.UploadBlobAsync(imageBytes, "users");
                    }
                }

                user.ImageId = imageId;
                
                var response = await _userHelper.UpdateUserAsync(user);

                if (!response.Succeeded)
                {
                    _toastNotification.Error("There was an error uploading the picture.", 7);
                    return new ObjectResult(new { Status = "fail" });
                }

                return new ObjectResult(new { Status = "success" });
            }


            return new ObjectResult(new { Status = "fail" });
        }

        public async Task<IActionResult> OrderDetails(int? id)
        {

            if(id == null)
            {
                return NotFound();
            }

            var order = await _orderRepository.GetCompleteOrderByIdAsync(id.Value);

            if(order == null)
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);

            if(user == null)
            {
                return NotFound();
            }

            if(order.Customer.Id != user.Id)
            {
                return RedirectToAction("NotAuthorized");
            }

            var orderDetails = await _orderRepository.GetOrderDetailsAsync(order.Id);

            if(orderDetails == null)
            {
                return NotFound();
            }

            List<OrderWithDetailsViewModel> customerOrders = new List<OrderWithDetailsViewModel>();
            customerOrders.Add(new OrderWithDetailsViewModel
            {
                Order = order,
                OrderDetails = orderDetails
            });

            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.CustomerOrders = customerOrders;
            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> RemoveFromWishlist(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetFullProduct(id.Value);

            if(product == null)
            {
                return NotFound();
            }

            var currentWishlist = await _productRepository.GetOrStartWishListAsync();
            var index = 0;
            
            for(int i = 0; i <= currentWishlist.Count(); i++)
            {
                if (currentWishlist[i].Id == product.Id)
                {
                    index = i;
                    break;
                }
            }

            currentWishlist.RemoveAt(index);         

            var result = _productRepository.UpdateWishlistCookie(currentWishlist);

            if(result.IsSuccess == false)
            {            
                currentWishlist.Add(product);
            }

            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.WishList = currentWishlist;

            return PartialView("_WishlistPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCartPartial()
        {
            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            return PartialView("_CartDropDownPartial",model);
        }

        public async Task<IActionResult> GoToWishList()
        {
            if (this.User.Identity.IsAuthenticated)
            {                
                return RedirectToAction("ViewUser",new {redirect = true});
            }
            else
            {
                var returnUrl = Url.Action("GoToWishList", "Account");
                _toastNotification.Information("You must login or Register first to continue.");
                return RedirectToAction("Login",new {returnUrl = returnUrl});
            }
        }

        public async Task<IActionResult> NotAuthorized()
        {
            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.Categories = await _categoryRepository.GetAllCategoriesAsync();
            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            return View(model);
        }

        public async Task<IActionResult> OrderDetailsByEmail(int? id,string userId,string returnUrl)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if(this.User.Identity.Name != user.UserName)
            {
                await _userHelper.LogoutAsync();
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }

            if (!this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }                     

            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderRepository.GetCompleteOrderByIdAsync(id.Value);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Customer.Id != user.Id)
            {
                return RedirectToAction("NotAuthorized");
            }

            var orderDetails = await _orderRepository.GetOrderDetailsAsync(order.Id);

            if (orderDetails == null)
            {
                return NotFound();
            }

            List<OrderWithDetailsViewModel> customerOrders = new List<OrderWithDetailsViewModel>();
            customerOrders.Add(new OrderWithDetailsViewModel
            {
                Order = order,
                OrderDetails = orderDetails
            });

            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.CustomerOrders = customerOrders;
            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;

            return View("OrderDetails",model);
        }
    }
}
