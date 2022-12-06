using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Webx.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly INotyfService _toastNotification;
        private readonly DataContext _dataContext;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IProductRepository _productRepository;

        public CategoryController(
            ICategoryRepository categoryRepository,
            INotyfService toastNotification,
            DataContext dataContext,
            IConverterHelper converterHelper,
            IImageHelper imageHelper,
            IBlobHelper blobHelper,
            IProductRepository productRepository
            )
        {
            _categoryRepository = categoryRepository;
            _toastNotification = toastNotification;
            _dataContext = dataContext;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _blobHelper = blobHelper;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Category> categories;

            // Get all Brands from the company:
            categories = await _categoryRepository.GetAllCategoriesAsync();

            //vai buscar as dataAnnotations da class Category para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Brand);
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();

            return View(categories);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                _toastNotification.Error("Category Id was not found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var category = await _categoryRepository.GetAllCategoriesByIdAsync(id.Value);

            if (category == null)
            {
                _toastNotification.Error("Category could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }

            CategoryViewModel model = new CategoryViewModel();
            if (category != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.CategoryToViewModel(category);
            }
            else
            {
                return null;
            }
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(CategoryViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var category = await _categoryRepository.GetAllCategoriesByIdAsync(model.Id);
                if (category == null)
                {
                    _toastNotification.Error("Error, the category was not found");
                    return RedirectToAction(nameof(ViewAll));
                }

                try
                {
                    Guid imageId = Guid.Empty;                  
                    if (model.PictureFile != null && model.PictureFile.Length > 0)
                    {
                        imageId = await _imageHelper.UploadImageAsync(model.PictureFile, model.ImageId, "categories");
                        category.ImageId = imageId;
                        model.ImageId = imageId;
                    }
                    else
                    {
                        category.ImageId = model.ImageId;
                    }
                    //model.ImageId = imageId;

                    category.Id = model.Id;
                    category.Name = model.Name;

                    _dataContext.Categories.Update(category);
                    await _dataContext.SaveChangesAsync();
                    model = _converterHelper.CategoryToViewModel(category);

                    _toastNotification.Success("Category changes saved successfully!!!");
                    ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
                    return View(model);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        _toastNotification.Error($"The Category Name:  {category.Name} , already exists!");
                    }
                    else
                    {
                        _toastNotification.Error($"There was a problem updating the employee!");
                    }
                    ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
                    return View(model);
                }

            };
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new CategoryViewModel();
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var category = _categoryRepository.GetAllCategoryByNameAsync(model.Name);

                if (category.Result != null)
                {
                    _toastNotification.Error("This Category Already Exists, Please try again...");
                    ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
                    return View(model);
                }

                if (category.Result == null)
                {
                    try
                    {
                        Guid imageId = Guid.Empty;
                        if (model.PictureFile != null && model.PictureFile.Length > 0)
                        {
                            imageId = await _imageHelper.UploadImageAsync(model.PictureFile, model.ImageId, "categories");
                        }
                        model.ImageId = imageId;

                        await _categoryRepository.AddCategoryAsync(model);
                        _toastNotification.Success("Category created successfully!!!");
                        ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
                        return View(model);
                    }
                    catch (Exception)
                    {
                        _toastNotification.Error("There was a problem, When try creating the category. Please try again");
                        ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
                        return View(model);
                    }
                }
            };
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Category/CategoryDetails")]
        public async Task<JsonResult> CategoryDetails(int? Id)
        {
            if (Id == null)
            {
                _toastNotification.Error("Category Id was not found.");
                return null;
            }

            var category = await _categoryRepository.GetAllCategoriesByIdAsync(Id.Value);
            CategoryViewModel model = new CategoryViewModel();

            if (category != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.CategoryToViewModel(category);
            }
            else
            {
                return null;
            }

            if (model == null)
            {
                return null;
            }

            var json = Json(model);
            return json;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Category/ToastNotification")]
        public JsonResult ToastNotification(string message, string type)
        {
            bool result = false;

            if (type == "success")
            {
                _toastNotification.Success(message, 5);
                result = true;
            }

            if (type == "error")
            {
                _toastNotification.Error(message, 5);
                result = true;
            }

            if (type == "warning")
            {
                _toastNotification.Warning(message, 5);
                result = true;
            }

            if (type == "Information")
            {
                _toastNotification.Information(message, 5);
                result = true;
            }

            return Json(result);
        }
    }
}
