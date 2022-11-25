using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public interface IUserHelper
    {
        Task CheckRoleAsync(string roleName);

        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<IdentityRole>GetUserRoleAsync(User user);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<IEnumerable<User>> GetAllEmployeesAsync();

        Task<IEnumerable<User>> GetAllActiveEmployeesAsync();

        Task<User> GetUserByIdAsync(string userId);

        Task<bool> CheckUserInRoleAsync(User user, string roleName);

        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);

        Task<IEnumerable<User>> GetAllCustomersUsersAsync();
        Task<IEnumerable<User>> GetAllActiveCustomersAsync();

        Task<List<User>> GetAllAdminUsersAsync();

        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        Task<List<User>> GetUsersInRoleAsync(string roleName);

        

        Task LogoutAsync();


        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);
       
        Task<User> GetUserByNIFAsync(string nIF);

        Task<string> GeneratePasswordResetTokenAsync(User user);

        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

        AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirect);

        Task<ExternalLoginInfo> GetExternalLoginInfoAsync();

        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent);

        Task<IdentityResult> UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo info);

        Task<IdentityResult> CreateAsync(User user);

        Task<IdentityResult> AddLoginAsync(User user, ExternalLoginInfo info);
        Task SignInAsync(User user, bool isPersistent);
        Task<bool> HasPasswordAsync(User user);

        Task<SignInResult> CheckPasswordAsync(User user, string oldPassword);

        IEnumerable<SelectListItem> GetEmployeesComboRoles();
        Task<IdentityRole> GetRoleByIdAsync(string roleId);
        Task<IdentityRole> GetRoleByNameAsync(string roleName);
        Task<IdentityResult> RemoveFromCurrentRoleAsync(User user,string currentRole);
        Task <SignInResult>FirstLoginAsync(User user);
        Task<User> GetUserByEmailWithCheckoutTempsAsync(string email);
    }
}
