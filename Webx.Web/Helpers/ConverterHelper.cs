﻿using Microsoft.CodeAnalysis.CSharp;
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
        private readonly IStatusRepository _statusRepository;

        public ConverterHelper(
            IUserHelper userHelper,
            IProductRepository productRepository,
            IStatusRepository statusRepository
            )
        {
            _userHelper = userHelper;
            _productRepository = productRepository;
            _statusRepository = statusRepository;
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

        public StoreViewModel StoreToViewModel(Store store)
        {
            return new StoreViewModel
            {
                Id = store.Id,
                Name = store.Name,
                PhoneNumber = store.PhoneNumber,
                Address = store.Address,
                IsActive = store.IsActive,
                City = store.City,
                Country = store.Country,
                Email = store.Email,
                IsOnlineStore = store.IsOnlineStore,
                ZipCode = store.ZipCode,
            };
        }


        public BrandViewModel BrandToViewModel(Brand brand)
        {
            return new BrandViewModel
            {
                Id = brand.Id,
                Name = brand.Name
            };
        }


        public CategoryViewModel CategoryToViewModel(Category category)
        {
            return new CategoryViewModel()
            {
                Id = category.Id,
                Name = category.Name,
                ImageId = category.ImageId,
            };
        }


        public ProductViewModel ProductToViewModel(Product product)
        {
            return new ProductViewModel()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsService = product.IsService,
                Brand = product.Brand,
                Category = product.Category,
                Brands = _productRepository.GetBrandsCombo(),
                Categories = _productRepository.GetCategoriesCombo(),
                Images = product.Images,
            };
        }

        public StockViewModel StockToViewModel(Stock stock)
        {
            return new StockViewModel()
            {
                Id = stock.Id,
                Product = stock.Product,
                Store = stock.Store,
                MinimumQuantity = stock.MinimumQuantity,
                Quantity = stock.Quantity,
            };
        }

        public EditCustomerViewModel ToEditCustomerViewModel(User user)
        {
            return new EditCustomerViewModel
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
            };
        }

        public OrderViewModel ToOrderViewModel(Order order)
        {
            return new OrderViewModel
            {
                Id = order.Id,
                Customer = order.Customer,
                Store = order.Store,
                Appointment = order.Appointment,
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                InvoiceId = order.InvoiceId,
                TotalQuantity = order.TotalQuantity,
                TotalPrice = order.TotalPrice,
                Status= order.Status,
                Statuses= _statusRepository.GetStatusesCombo(),
            };
        }

    }
}
