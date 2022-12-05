using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public class ReviewsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserHelper _userHelper;
        private readonly INotyfService _toastNotification;
        private readonly IBrandRepository _brandRepository;

        public ReviewsController(IProductRepository productRepository, IUserHelper userHelper, INotyfService toastNotification, IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _userHelper = userHelper;
            _toastNotification = toastNotification;
            _brandRepository = brandRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(int productId, string userId)
        {
            var user = await _userHelper.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetFullProduct(productId);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;

            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            var productReviewModel = new ProductReviewViewModel
            {
                Product = product,
                User = user,
                UserId = user.Id,
                ProductId = productId
            };

            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
            model.ProductReviewViewModel = productReviewModel;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopViewModel model)
        {
            var product = await _productRepository.GetFullProduct(model.ProductReviewViewModel.ProductId);
            var user = await _userHelper.GetUserByIdAsync(model.ProductReviewViewModel.UserId);

            if (product == null || user == null)
            {
                return NotFound();
            }


            if (this.ModelState.IsValid)
            {

                var review = new ProductReview
                {
                    User = user,
                    Product = product,
                    WouldRecommend = model.ProductReviewViewModel.WouldRecommend,
                    ReviewDate = DateTime.UtcNow,
                    ReviewText = model.ProductReviewViewModel.ReviewText,
                    ReviewTitle = model.ProductReviewViewModel.ReviewTitle,
                    Rating = model.ProductReviewViewModel.Rating,
                    Status = "Unauthorized"
                };

                try
                {
                    await _productRepository.CreateReviewAsync(review);
                    _toastNotification.Success($"Thank you for the review {user.FullName} !, your review will be analyzed by our staff and will be available as soon as possible.", 10);
                }
                catch (Exception ex)
                {
                    _toastNotification.Error($"There was a problem saving your review, please try again later.", 10);
                    ViewBag.UserFullName = user.FullName;
                    ViewBag.IsActive = user.Active;
                    ViewBag.WouldRecommed = model.ProductReviewViewModel.WouldRecommend;
                    ViewBag.Rating = model.ProductReviewViewModel.Rating;

                    model.Cart = await _productRepository.GetCurrentCartAsync();
                    model.ProductReviewViewModel.User = user;
                    model.ProductReviewViewModel.Product = product;
                    model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
                    model.WishList = await _productRepository.GetOrStartWishListAsync();
                    return View(model);
                }


                return RedirectToAction("ProductInfo", "Products", new { id = model.ProductReviewViewModel.ProductId });
            }
            else
            {
                List<string> errors = new List<string>();

                foreach (var item in this.ModelState.Values)
                {
                    if (item.ValidationState == ModelValidationState.Invalid)
                    {
                        errors.Add(item.Errors.FirstOrDefault().ErrorMessage);
                    }
                }

                foreach (string error in errors)
                {
                    _toastNotification.Error(error);
                }
            }

            ViewBag.UserFullName = user.FullName;
            ViewBag.IsActive = user.Active;
            ViewBag.WouldRecommed = model.ProductReviewViewModel.WouldRecommend;
            ViewBag.Rating = model.ProductReviewViewModel.Rating;

            model.Cart = await _productRepository.GetCurrentCartAsync();
            model.ProductReviewViewModel.User = user;
            model.ProductReviewViewModel.Product = product;
            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            return View(model);


        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id.Value <= 0)
            {
                return NotFound();
            }

            var customerReview = await _productRepository.GetProductReviewByIdAsync(id.Value);

            if (customerReview == null)
            {
                return NotFound();
            }

            var thisUser = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if (thisUser == null)
            {
                return NotFound();
            }

            if (thisUser != customerReview.User)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ViewBag.UserFullName = customerReview.User.FullName;
            ViewBag.IsActive = customerReview.User.Active;
            ViewBag.WouldRecommed = customerReview.WouldRecommend;
            ViewBag.Rating = customerReview.Rating;
            ViewBag.IsEdit = true;

            var model = await _productRepository.GetInitialShopViewModelAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            var productReviewModel = new ProductReviewViewModel
            {
                Product = customerReview.Product,
                User = customerReview.User,
                UserId = customerReview.User.Id,
                ProductId = customerReview.Product.Id,
                Rating = customerReview.Rating,
                ReviewText = customerReview.ReviewText,
                ReviewTitle = customerReview.ReviewTitle,
                WouldRecommend = customerReview.WouldRecommend,
                Id = customerReview.Id,
            };

            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
            model.ProductReviewViewModel = productReviewModel;

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShopViewModel model)
        {
            var customerReview = await _productRepository.GetProductReviewByIdAsync(model.ProductReviewViewModel.Id);

            if (customerReview == null)
            {
                return NotFound();
            }


            if (this.ModelState.IsValid)
            {

                customerReview.WouldRecommend = model.ProductReviewViewModel.WouldRecommend;
                customerReview.ReviewDate = DateTime.UtcNow;
                customerReview.ReviewText = model.ProductReviewViewModel.ReviewText;
                customerReview.ReviewTitle = model.ProductReviewViewModel.ReviewTitle;
                customerReview.Rating = model.ProductReviewViewModel.Rating;
                customerReview.Status = "Unauthorized";


                try
                {
                    await _productRepository.UpdateReviewAsync(customerReview);
                    _toastNotification.Success($"Your review as been updated {customerReview.User.FullName}!, it will be analyzed by our staff and will be available as soon as possible.", 10);
                }
                catch (Exception ex)
                {
                    _toastNotification.Error($"There was a problem updating your review, please try again later.", 10);
                    ViewBag.UserFullName = customerReview.User.FullName;
                    ViewBag.IsActive = customerReview.User.Active;
                    ViewBag.WouldRecommed = model.ProductReviewViewModel.WouldRecommend;
                    ViewBag.Rating = model.ProductReviewViewModel.Rating;

                    model.Cart = await _productRepository.GetCurrentCartAsync();
                    model.ProductReviewViewModel.User = customerReview.User;
                    model.ProductReviewViewModel.Product = customerReview.Product;
                    model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
                    model.WishList = await _productRepository.GetOrStartWishListAsync();
                    return View(model);
                }


                return RedirectToAction("ProductInfo", "Products", new { id = model.ProductReviewViewModel.ProductId });
            }
            else
            {
                List<string> errors = new List<string>();

                foreach (var item in this.ModelState.Values)
                {
                    if (item.ValidationState == ModelValidationState.Invalid)
                    {
                        errors.Add(item.Errors.FirstOrDefault().ErrorMessage);
                    }
                }

                foreach (string error in errors)
                {
                    _toastNotification.Error(error);
                }
            }

            ViewBag.UserFullName = customerReview.User.FullName;
            ViewBag.IsActive = customerReview.User.Active;
            ViewBag.WouldRecommed = model.ProductReviewViewModel.WouldRecommend;
            ViewBag.Rating = model.ProductReviewViewModel.Rating;

            model.Cart = await _productRepository.GetCurrentCartAsync();
            model.ProductReviewViewModel.User = customerReview.User;
            model.ProductReviewViewModel.Product = customerReview.Product;
            model.Brands = (List<Brand>)await _brandRepository.GetAllBrandsAsync();
            model.WishList = await _productRepository.GetOrStartWishListAsync();

            return View(model);
        }

        [Authorize(Roles = "Admin,Product Manager")]
        public async Task<IActionResult> Validate()
        {
            List<ProductReview> reviews = await _productRepository.GetAllReviewsAsync();
            reviews = reviews.OrderByDescending(r => r.Id).ToList();

            if (reviews == null)
            {
                reviews = new List<ProductReview>();
            }

            ViewBag.Type = typeof(ProductReview);

            return View(reviews);

        }

        [HttpPost]
        [Route("Reviews/ChangeStatus")]
        public async Task<JsonResult> ChangeStatus(int id, string status)
        {
            bool result = false;

            if (id <= 0)
            {
                return Json(result);
            }

            if (string.IsNullOrEmpty(status))
            {
                return Json(result);
            }

            var review = await _productRepository.GetProductReviewByIdAsync(id);

            if (review == null)
            {
                return Json(result);
            }

            review.Status = status;

            try
            {
                await _productRepository.UpdateReviewAsync(review);
                _toastNotification.Success("Review was updated with success!", 5);
                result = true;
                return Json(result);
            }
            catch (Exception ex)
            {
                _toastNotification.Error($"There was a problem updating the review status. {ex.InnerException}", 10);
            }

            return Json(result);
        }

        [HttpPost]
        [Route("Reviews/GetReview")]
        public async Task<JsonResult> GetReview(int id)
        {
            if(id <= 0)
            {
                return Json(null);
            }

            var review = await _productRepository.GetProductReviewByIdAsync(id);

            if(review == null)
            {
                return Json(null);
            }

            return Json(review);

        }
    }
}
