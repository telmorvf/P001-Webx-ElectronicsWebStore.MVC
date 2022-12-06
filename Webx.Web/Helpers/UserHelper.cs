using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UserHelper(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName,
                });
            }
        }

        public async Task<IdentityResult> AddUserToRoleAsync(User user, string roleName)
        {
            return await _userManager.AddToRoleAsync(user, roleName);            
        }

        public async Task<bool> CheckUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);        
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IEnumerable<User>> GetAllCustomersUsersAsync()
        {

            var users = await _userManager.Users.ToListAsync();
            var customers = new List<User>();

            foreach(var user in users)
            {
                if (await CheckUserInRoleAsync(user, "Customer"))
                {
                    customers.Add(user);
                }
            }

            return customers;
        }

        public async Task<IEnumerable<User>> GetAllActiveCustomersAsync()
        {
            var customers = await GetAllCustomersUsersAsync();

            return customers.Where(c => c.Active == true);
        }

        public async Task<List<User>> GetAllAdminUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var admins = new List<User>();

            foreach (var user in users)
            {
                if (await CheckUserInRoleAsync(user, "Admin"))
                {
                    admins.Add(user);
                }
            }

            return admins;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);           
        }

        public async Task<List<User>> GetUsersInRoleAsync(string roleName)
        {
            return (List<User>)await _userManager.GetUsersInRoleAsync(roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {            
            return await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task<User> GetUserByNIFAsync(string nIF)
        {
            return await _userManager.Users.Where(u => u.NIF == nIF).FirstOrDefaultAsync();
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirect)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirect);
        }

        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            return info;
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent)
        {
            return await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent);
        }

        public async Task<IdentityResult> UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo info)
        {
            return await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
        }

        public async Task<IdentityResult> CreateAsync(User user)
        {
            return await _userManager.CreateAsync(user);
        }

        public async Task<IdentityResult> AddLoginAsync(User user, ExternalLoginInfo info)
        {
            return await _userManager.AddLoginAsync(user, info);
        }

        public async Task SignInAsync(User user, bool isPersistent)
        {
            await _signInManager.SignInAsync(user, isPersistent);
        }

        public async Task<bool> HasPasswordAsync(User user)
        {
            return await _userManager.HasPasswordAsync(user);
        }

        public async Task<SignInResult> CheckPasswordAsync(User user, string oldPassword)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, oldPassword, false);
        }

        public async Task<IEnumerable<User>> GetAllEmployeesAsync()
        {
            var employees = new List<User>();
            employees.AddRange(await _userManager.GetUsersInRoleAsync("Technician"));
            employees.AddRange(await _userManager.GetUsersInRoleAsync("Product Manager"));
            employees.AddRange(await _userManager.GetUsersInRoleAsync("Admin"));

            return employees;
        }

        public async Task<IEnumerable<User>> GetAllActiveEmployeesAsync()
        {
            var employees = new List<User>();
            employees.AddRange(await _userManager.GetUsersInRoleAsync("Technician"));
            employees.AddRange(await _userManager.GetUsersInRoleAsync("Product Manager"));
            employees.AddRange(await _userManager.GetUsersInRoleAsync("Admin"));

            return employees.Where(e => e.Active == true);
        }

        public IEnumerable<SelectListItem> GetEmployeesComboRoles()
        {
            var list = _roleManager.Roles;
            var comboRolesList = new List<SelectListItem>();

            foreach(var role in list)
            {
                if(role.Name != "Customer")
                {
                    comboRolesList.Add(new SelectListItem
                    {
                        Text = role.Name,
                        Value = role.Id
                    });
                }
            }

            comboRolesList.Add(new SelectListItem
            {
                Text = "[Select Role]",
                Value = "0"
            });

            return comboRolesList.OrderBy(l => l.Text);
            
        }

        public async Task<IdentityRole> GetUserRoleAsync(User user)
        {
            var userRole = await _userManager.GetRolesAsync(user);

            return await _roleManager.Roles.Where(role => role.Name == userRole.FirstOrDefault()).FirstOrDefaultAsync();
        }

        public async Task<IdentityRole> GetRoleByIdAsync(string roleId)
        {
            return await _roleManager.FindByIdAsync(roleId);
        }

        public async Task<IdentityRole> GetRoleByNameAsync(string roleName)
        {
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<IdentityResult> RemoveFromCurrentRoleAsync(User user,string currentRole)
        {
            return await _userManager.RemoveFromRoleAsync(user, currentRole);
        }

        public async Task<SignInResult> FirstLoginAsync(User user)
        {

            try
            {
                await _signInManager.SignInAsync(user, false);
                return SignInResult.Success;
            }
            catch (System.Exception)
            {
                return SignInResult.Failed;
            }          
        }

        public async Task<User> GetUserByEmailWithCheckoutTempsAsync(string email)
        {
            return await _userManager.Users.Include(u=> u.CheckoutTempData).Where(u => u.Email == email).FirstOrDefaultAsync();
        }

      
    }
}
