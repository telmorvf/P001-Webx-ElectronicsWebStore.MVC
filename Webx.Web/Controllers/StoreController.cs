using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Schedule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Controllers
{
    public class StoreController : Controller
    {
        private readonly IStoreRepository _storeRepository;
        private readonly INotyfService _toastNotification;
        private readonly DataContext _dataContext;
        private readonly IConverterHelper _converterHelper;

        public StoreController(
            IStoreRepository storeRepository,
            INotyfService toastNotification,
            DataContext dataContext,
            IConverterHelper converterHelper
            )
        {
            _storeRepository = storeRepository;
            _toastNotification = toastNotification;
            _dataContext = dataContext;
            _converterHelper = converterHelper;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewAll(bool isActive)
        {
            IEnumerable<Store> stores;

            // Get all Stores from the company:
            // Online store, physical stores and if they are active or inactive
            if (isActive)
            {
                stores = await _storeRepository.GetAllActiveStoresAsync();
            }
            else
            {
                stores = await _storeRepository.GetAllStoresAsync();
            }

            ViewBag.IsActive = isActive;
            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Store);

            return View(stores);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                _toastNotification.Error("Store Id was not found.");
                return RedirectToAction(nameof(ViewAll));
            }

            var store = await _storeRepository.GetAllStoreByIdAsync(id.Value);

            if (store == null)
            {
                _toastNotification.Error("Store could not be found.");
                return RedirectToAction(nameof(ViewAll));
            }

            StoreViewModel model = new StoreViewModel();
            if (store != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.StoreToViewModel(store);
            }
            else
            {
                return null;
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(StoreViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                if (model.Name == null)
                {
                    _toastNotification.Error("Error, the store was not found");
                    return RedirectToAction(nameof(ViewAll));
                }

                try
                {
                    var store = _converterHelper.StoreFromViewModel(model, false);
                    await _storeRepository.UpdateAsync(model);

                    _toastNotification.Success("Store changes saved successfully!!!");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        _toastNotification.Error($"The nif  {model.Name}  already exists!");
                    }
                    else
                    {
                        _toastNotification.Error($"There was a problem updating the employee!");
                    }
                    return View(model);
                }

            };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var model = new StoreViewModel();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoreViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var storeName = _storeRepository.GetAllStoreByNameAsync(model.Name);
                if (storeName.Result != null)
                {
                    _toastNotification.Error("This Store Name Already Exists, Please try again...");
                    return View(model);
                }
                else
                {
                    try
                    {
                        Store newBrand = _converterHelper.StoreFromViewModel(model, true);
                        await _storeRepository.CreateAsync(model);
                        _toastNotification.Success("Store created successfully!!!");
                        return View(model);
                    }
                    catch (Exception)
                    {
                        _toastNotification.Error("There was a problem, When try creating the store. Please try again");
                        return View(model);
                    }   
                }
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reactivate(int? id)
        {
            if (id == null)
            {
                _toastNotification.Error("There was a problem resolving the store id. Try again later");
                return RedirectToAction("ViewAll", "Store", new { isActive = true });
            }

            var store = await _storeRepository.GetAllStoreByIdAsync((int)id);

            if (store == null)
            {
                _toastNotification.Error("There was a problem getting the user data. Try again later");
                return RedirectToAction("ViewAll", "Store", new { isActive = true });
            }

            store.IsActive = true;
            try
            {
                await _storeRepository.UpdateAsync(store);
                _toastNotification.Success($"{store.Name} Store was reactivated with success!");
                return RedirectToAction("Update", "Store", new { id = store.Id });
            }
            catch (Exception)
            {
                _toastNotification.Error("There was a problem saving the store data. Try again later");
                return RedirectToAction("Update", "Employee", new { id = store.Id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Store/StoreDetails")]
        public async Task<JsonResult> StoreDetails(int? Id)
        {
            if (Id == null)
            {
                _toastNotification.Error("Store Id was not found.");
                return null;
            }

            var store = await _storeRepository.GetAllStoreByIdAsync(Id.Value);
            StoreViewModel model = new StoreViewModel();

            if (store != null)
            {
                // ConverterHelper: converte from class to ViewModel
                model = _converterHelper.StoreToViewModel(store);
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
        [Route("Store/Delete")]
        public async Task<JsonResult> Delete(int? Id)
        {
            bool result = false;

            if (Id == null)
            {
                return Json(result);
            }

            var store = await _storeRepository.GetAllStoreByIdAsync(Id.Value);

            try
            {            
                store.IsActive = false;
                result = true;
                await _storeRepository.UpdateAsync(store);
            }
            catch (Exception)
            {
                _toastNotification.Error("There was a problem saving the store data. Try again later");
            }

            return Json(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Store/ToastNotification")]
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
