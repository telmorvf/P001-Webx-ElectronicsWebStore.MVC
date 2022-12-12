using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.EJ2.PdfViewer;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webx.Web.Controllers
{
    public class StockController : Controller
    {
        private readonly INotyfService _toastNotification;
        private readonly IConverterHelper _converterHelper;
        private readonly DataContext _dataContext;
        private readonly IImageHelper _imageHelper;
        private readonly IUserHelper _userHelper;
        private readonly IXMLHelper _xMLHelper;
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;

        public StockController(
            INotyfService toastNotification,
            IConverterHelper converterHelper,
            DataContext dataContext,
            IImageHelper imageHelper,
            IUserHelper userHelper,
            IXMLHelper xMLHelper,
            IStockRepository stockRepository,
            IProductRepository productRepository
            )
        {
            _toastNotification = toastNotification;
            _converterHelper = converterHelper;
            _dataContext = dataContext;
            _imageHelper = imageHelper;
            _userHelper = userHelper;
            _xMLHelper = xMLHelper;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }


        [Authorize(Roles = "Admin, Product Manager, Technician")]
        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Stock> stocks;

            // Get all Brands from the company:
            stocks = await _stockRepository.GetStockAllAsync();

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            //ViewData["Title"] = "View All Stock";
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            //ViewBag.Type = typeof(Brand);
            return View("ViewAll", stocks);
        }

        [Authorize(Roles = "Admin, Product Manager, Technician")]
        public async Task<IActionResult> ViewAllAlert()
        {
            IEnumerable<Stock> stocksAlerts;

            // Get all Brands from the company:
            stocksAlerts = await _stockRepository.GetStockAlerts();

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Brand);
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            //ViewData["Title"] = "View All Stock Alerts";
            return View("ViewAllAlert",stocksAlerts);
        }

        [HttpGet]
        [Route("Stock/CountStockAlerts")]
        [Authorize(Roles = "Admin, Product Manager, Technician")]
        public async Task<JsonResult> CountStockAlerts()
        {
            var count = await _stockRepository.GetStockAlertsCount();

            return Json(count);
        }

        [Authorize(Roles = "Admin, Product Manager")]
        public async Task<FileResult> ExportXml()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\stockAlert\");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(basePath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }

            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
            if (user == null)
            {
                return null;
            }

            var products = await _stockRepository.GetStockAlerts();
            if (products == null)
            {
                return null;
            }

            if (products.Count() == 0)
            {
                return null;
            }
            var fileName = _xMLHelper.GenerateXML(products, user.FullName);

            //Build the File Path.
            string path = basePath + fileName;

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(path);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fileName);
        }

        [Authorize(Roles = "Admin, Product Manager")]
        public async Task<IActionResult> Update(int? id)
        {
            //var viewData = id;
            
            if (id == null)
            {
                _toastNotification.Error("Stock Id was not found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var stock = await _stockRepository.GetStockByIdAsync(id.Value);

            if (stock == null)
            {
                _toastNotification.Error("Stock could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }

            StockViewModel model = new StockViewModel();
            if (stock != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.StockToViewModel(stock);

                //if (viewData < 0)
                //    ViewData["Title"] = "View All Stock";
                
                //else
                //    ViewData["Title"] = "View All Stock Alerts";
            }
            else
            {
                return null;
            }
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Product Manager")]
        public async Task<IActionResult> Update(StockViewModel model)
        {
            var stock = await _stockRepository.GetStockByIdAsync(model.Id);
            if (this.ModelState.IsValid)
            {
                
                if (stock == null)
                {
                    _toastNotification.Error("Error, the Stocks was not found");
                    return RedirectToAction(nameof(ViewAll));
                }

                try
                {
                    stock.Id = model.Id;
                    stock.Quantity = model.Quantity;
                    stock.MinimumQuantity = model.MinimumQuantity;

                    //model.ImageFirst = stock.Product.ImageFirst;
                    _dataContext.Stocks.Update(stock);
                    await _dataContext.SaveChangesAsync();

                    _toastNotification.Success("Stock changes saved successfully!!!");

                    model = _converterHelper.StockToViewModel(stock);
                    ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
                    return View(model);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        _toastNotification.Error($"The Stock Quantity already exists!");
                    }
                    else
                    {
                        _toastNotification.Error($"There was a problem updating the Stock Quantities!");
                    }
                    model = _converterHelper.StockToViewModel(stock);
                    ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
                    return View(model);
                }
            };
            model = _converterHelper.StockToViewModel(stock);
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin, Product Manager, Technician")]
        [HttpPost]
        [Route("Stock/ToastNotification")]
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
