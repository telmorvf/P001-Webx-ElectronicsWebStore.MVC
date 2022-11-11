using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Models;

namespace Webx.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly IUserHelper _userHelper;
        private readonly IProductRepository _productRepository;

        public ConverterHelper(IUserHelper userHelper,IProductRepository productRepository)
        {
            _userHelper = userHelper;
            _productRepository = productRepository;
        }

        public async Task<List<CartViewModel>> ToCartViewModelAsync(List<CookieItemModel> cookieItemList)
        {
            List<CartViewModel> cart = new List<CartViewModel>();

            foreach(var item in cookieItemList)
            {
                cart.Add(new CartViewModel
                {
                    Product = await _productRepository.GetFullProduct(item.ProductId),
                    Quantity = item.Quantity,
                });
            }

            return cart;
        }

        public async Task<EditEmployeeViewModel> ToEditEmployeeViewModelAsync(User user)
        {
            var userRole = await _userHelper.GetUserRoleAsync(user);

            return new EditEmployeeViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                AccessFailedCount = user.AccessFailedCount,
                Active = user.Active,
                ConcurrencyStamp = user.ConcurrencyStamp,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Id = user.Id,
                ImageId = user.ImageId,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                NIF = user.NIF,
                NormalizedEmail = user.NormalizedEmail,
                NormalizedUserName = user.NormalizedUserName,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,               
                Roles = _userHelper.GetEmployeesComboRoles(),
                CurrentRole = userRole.Name
            };
        }
    }
}
