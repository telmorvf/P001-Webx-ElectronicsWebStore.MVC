using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;

namespace Webx.Web.Controllers
{
    public class StockController : Controller
    {
        private readonly IStockRepository _stockRepository;

        public StockController(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Stock> stocks;

            // Get all Brands from the company:
            stocks = await _stockRepository.GetAllStockAsync();

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Brand);

            return View(stocks);
        }
    }
}
