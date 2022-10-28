using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(IUserHelper userHelper,IMailHelper mailHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
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
        public IActionResult Register()
        {
            return View();
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
                            token = userToken
                        }, protocol: HttpContext.Request.Scheme);
                        var returnLink = Url.Action("Index", "Home", null, protocol: HttpContext.Request.Scheme);

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

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
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
        public IActionResult ExternalLogin(string provider, string returnurl = null)
        {
            var redirect = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnurl });
            var properties = _userHelper.ConfigureExternalAuthenticationProperties(provider, redirect);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnurl = null, string remoteError = null)
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
                if (returnurl != null)
                {
                    return LocalRedirect(returnurl);
                }
                else return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["ReturnUrl"] = returnurl;
                ViewData["ProvierDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string? returnurl = null)
        {
            returnurl = returnurl ?? Url.Content("~/");

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
                        return LocalRedirect(returnurl);
                    }
                }
                ModelState.AddModelError("Email", "User already exists");
            }

            ViewData["ReturnUrl"] = returnurl;
            return View(model);
        }

    }
}
