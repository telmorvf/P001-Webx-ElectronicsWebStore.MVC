using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Repositories;
using Webx.Web.Helpers;

namespace Webx.Web.Controllers
{
    [Authorize(Roles = "Admin, Technician, Product Manager")]

    public class AdminPanelController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IOrderRepository _orderRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;

        public AdminPanelController(
            IUserHelper userHelper,
            IOrderRepository orderRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository
            )
        {
            _userHelper = userHelper;
            _orderRepository = orderRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            //this month sales graphic data
            var thisMonthsales = await _orderRepository.GetMonthlySales(DateTime.UtcNow.Month);

            // OverallMonthsSales
            // MonthlySales
            // OverallMonthsSales

            ViewBag.MonthlySales = thisMonthsales;
            ViewBag.MonthName = thisMonthsales.FirstOrDefault().Month;
            string[] color1 = { "#efcfe3" };
            string[] color2 = { "#ea9ab2" };
            string[] color3 = { "#e27396" };
            ViewBag.color1 = color1;
            ViewBag.color2 = color2;
            ViewBag.color3 = color3;

            //all months overal sales graphic data
            var overallMonthsSales = await _orderRepository.GetYearSalesByMonthAsync(DateTime.UtcNow.Year);
            ViewBag.OverallMonthsSales = overallMonthsSales; 

            //total users data
            ViewBag.totalUsers = await _userHelper.GetTotalUsersAsync();

            //active work orders data
            ViewBag.UnshippedOrders = await _orderRepository.GetUnshippedOrdersCount();

            //Total Registered brands
            ViewBag.totalBrands = await _brandRepository.GetTotalBrandsSoldAsync();

            //Most sold Service
            var categories = await _categoryRepository.GetMostSoldCategoriesData();
            ViewBag.mostSoldCategory = categories.Last().Name;

            return View();
        }

        ////Active Opend Orders
        //[HttpPost]
        //[Route("AdminPanel/GetOpenedWorkOrders")]
        //public async Task<JsonResult> GetOpenedWorkOrders()
        //{
        //    var OpenedWorkOrders = await _workOrderRepository.GetOpenedWorkOrdersAsync();
        //    var json = Json(OpenedWorkOrders);
        //    return json;
        //}


        [HttpPost]
        [Route("AdminPanel/GetChartUserData")]
        public async Task<JsonResult> GetChartUserData()
        {
            var registeredUserData = await _userHelper.GetUsersChartDataAsync();
            var json = Json(registeredUserData);
            return json;
        }


        [HttpPost]
        [Route("AdminPanel/GetUnshippedOrdersChart")]
        public async Task<JsonResult> GetUnshippedOrdersChart()
        {
            var unshippedOrdersData = await _orderRepository.GetUnshippedOrdersChartAsync(DateTime.UtcNow.Month);
            var json = Json(unshippedOrdersData);
            return json;
        }


        [HttpPost]
        [Route("AdminPanel/GetBrandsChartData")]
        public async Task<JsonResult> GetBrandsChartData()
        {
            var brandsData = await _brandRepository.GetBrandsChartDataAsync();
            var json = Json(brandsData);
            return json;
        }


        [HttpPost]
        [Route("AdminPanel/GetCategoriesChartData")]
        public async Task<JsonResult> GetCategoriesChartData()
        {
            var categoriesData = await _categoryRepository.GetMostSoldCategoriesData();
            var json = Json(categoriesData);
            return json;
        }


    }
}
