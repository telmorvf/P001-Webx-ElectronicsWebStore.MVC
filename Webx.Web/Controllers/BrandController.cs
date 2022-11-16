using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Controllers
{
    public class BrandController : Controller
    {
        private readonly IBrandRepository _brandRepository;
        private readonly INotyfService _toastNotification;
        private readonly DataContext _dataContext;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;

        public BrandController(
            IBrandRepository brandRepository,
            INotyfService toastNotification,
            DataContext dataContext,  
            IConverterHelper converterHelper,
            IImageHelper imageHelper
            )
        {
            _brandRepository = brandRepository;
            _toastNotification = toastNotification;
            _dataContext = dataContext;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
        }

        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Brand> brands;

            // Get all Brands from the company:
            brands = await _brandRepository.GetAllBrandsAsync();

            //vai buscar as dataAnnotations da class Brand para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Brand);

            return View(brands);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                _toastNotification.Error("Brand Id was not found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var brand = await _brandRepository.GetBrandByIdAsync(id.Value);

            if (brand == null)
            {
                _toastNotification.Error("Brand could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }

            BrandViewModel model = new BrandViewModel();
            if (brand != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.BrandToViewModel(brand);
            }
            else
            {
                return null;
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(BrandViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var brand = await _brandRepository.GetBrandByIdAsync(model.Id);
                if (brand == null)
                {
                    _toastNotification.Error("Error, the brand was not found");
                    return View(model);
                };

                try
                {
                    Guid imageId = Guid.Empty;
                    if (model.PictureFile != null && model.PictureFile.Length > 0)
                    {
                        //Convert image bit array and upload to Azure
                        imageId = await _imageHelper.UploadImageAsync(model.PictureFile, model.ImageId, "brands");
                        brand.ImageId = imageId;
                        model.ImageId = imageId;
                    }
                    else
                    {
                        brand.ImageId = model.ImageId;
                    }
                    //model.ImageId = imageId;

                    brand.Id = model.Id;
                    brand.Name = model.Name;

                    _dataContext.Brands.Update(brand);
                    await _dataContext.SaveChangesAsync();
                    model = _converterHelper.BrandToViewModel(brand);

                    _toastNotification.Success("Brand changes saved successfully!!!");                 
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        _toastNotification.Error($"The brand  {model.Name}  already exists!");
                    }
                    else
                    {
                        _toastNotification.Error($"There was a problem updating the brand, try again later!");
                    }

                    return View(model);
                }
            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new BrandViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var brand = _brandRepository.GetBrandByNameAsync(model.Name);
                if (brand.Result != null)
                {
                    _toastNotification.Error("This Brand Already Exists, Please try again...");
                    return View(model);
                }

                if (brand.Result == null)
                {
                    try
                    {
                        Guid imageId = Guid.Empty;
                        if (model.PictureFile != null && model.PictureFile.Length > 0)
                        {
                            //Convert image bit array and upload to Azure
                            imageId = await _imageHelper.UploadImageAsync(model.PictureFile, model.ImageId, "brands");
                        }
                        model.ImageId = imageId;

                        // Convert from Model to Entitie Brand, and new? = true
                        Brand newBrand = _converterHelper.BrandFromViewModel(model, true);
                        await _brandRepository.CreateAsync(newBrand);
                        _toastNotification.Success("Brand created successfully!!!");
                        return View(model);
                    }
                    catch (Exception)
                    {
                        _toastNotification.Error("There was a problem, When try creating the category. Please try again");
                        return View(model);
                    }
                }
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Brand/BrandDetails")]
        public async Task<JsonResult> BrandDetails(int? Id)
        {
            if (Id == null)
            {
                _toastNotification.Error("Brand Id was not found.");
                return null;
            }

            var brand = await _brandRepository.GetBrandByIdAsync(Id.Value);
            BrandViewModel model = new BrandViewModel();

            if (brand != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.BrandToViewModel(brand);
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
        [Route("Brand/ToastNotification")]
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
