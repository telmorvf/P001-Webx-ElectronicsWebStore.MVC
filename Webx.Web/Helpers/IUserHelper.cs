using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Helpers
{
    public interface IUserHelper
    {
        Task CheckRoleAsync(string roleName);

        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<User> GetUserByIdAsync(string userId);

        Task<bool> CheckUserInRoleAsync(User user, string roleName);

        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);

        Task<IEnumerable<User>> GetAllCustomersUsersAsync();

        Task<List<User>> GetAllAdminUsersAsync();

        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        Task<List<User>> GetUsersInRoleAsync(string roleName);
    }
}
