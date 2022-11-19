using Microsoft.CodeAnalysis.CSharp;
using System;
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

        public ConverterHelper(
            IUserHelper userHelper,
            IProductRepository productRepository
            )
        {
            _userHelper = userHelper;
            _productRepository = productRepository;
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

        public Store StoreFromViewModel(StoreViewModel model, bool isNew)
        {
            return new Store
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                IsActive = model.IsActive,
                City = model.City,
                Country = model.Country,
                Email = model.Email,
                IsOnlineStore = model.IsOnlineStore,
                ZipCode = model.ZipCode,
            };
        }


        public BrandViewModel BrandToViewModel(Brand brand)
        {
            return new BrandViewModel
            {
                Id = brand.Id,
                Name = brand.Name,
                ImageId = brand.ImageId,
            };
        }

        public Brand BrandFromViewModel(BrandViewModel model, bool isNew)
        {
            return new Brand
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                ImageId = model.ImageId,
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

        public Category CategoryFromViewModel(CategoryViewModel model, bool isNew)
        {
            return new Category
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                ImageId= model.ImageId,
            };
        }


        public ProductViewModel ProductToViewModel(Product product)
        {
            return new ProductViewModel()
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsService = product.IsService,
                ImageFirst = product.ImageFirst,
                BrandId = product.BrandId.ToString(),
                BrandName = product.Brand.Name,
                Brands = _productRepository.GetBrandsCombo(product.BrandId),
                CategoryId = product.CategoryId.ToString(),
                CategoryName = product.Category.Name,
                Categories = _productRepository.GetCategoriesCombo(product.CategoryId),

            };
        }
        public Product ProductFromViewModel(ProductViewModel model, bool isNew)
        {
            return new Product
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                IsService = model.IsService,
 
                CategoryId = Convert.ToInt32(model.CategoryId),
                BrandId = Convert.ToInt32(model.BrandId),
            };
        }
        public ProductAddViewModel ProductAddToViewModel(Product product)
        {
            return new ProductAddViewModel()
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsService = product.IsService,
                ImageFirst = product.ImageFirst,
                BrandId = product.BrandId.ToString(),
                Brands = _productRepository.GetBrandsCombo(product.BrandId),
                CategoryId = product.CategoryId.ToString(),
                Categories = _productRepository.GetCategoriesCombo(product.CategoryId),
            };
        }
        public Product ProductAddFromViewModel(ProductAddViewModel model, bool isNew)
        {
            return new Product
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                IsService = model.IsService,
                CategoryId = Convert.ToInt32(model.CategoryId),
                BrandId = Convert.ToInt32(model.BrandId),
            };
        }

        public ServiceViewModel ServiceToViewModel(Product product)
        {
            return new ServiceViewModel()
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsService = product.IsService,
                ImageFirst = product.ImageFirst,
                BrandId = product.BrandId.ToString(),
                Brands = _productRepository.GetBrandsCombo(product.BrandId),
                CategoryId = product.CategoryId.ToString(),
                Categories = _productRepository.GetCategoriesCombo(product.CategoryId),
            };
        }
        public Product ServiceFromViewModel(ServiceViewModel model, bool isNew)
        {
            return new Product
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                IsService = model.IsService,

                CategoryId = Convert.ToInt32(model.CategoryId),
                BrandId = Convert.ToInt32(model.BrandId),
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
        public Stock StockFromViewModel(StockViewModel model, bool isNew)
        {
            return new Stock
            {
                Id = isNew ? 0 : model.Id,
                Store = model.Store,
                Product = model.Product,
                MinimumQuantity = model.MinimumQuantity,
                Quantity = model.Quantity,
            };
        }

    }
}
