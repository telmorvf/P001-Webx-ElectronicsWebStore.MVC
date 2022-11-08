using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;

namespace Webx.Web.Controllers
{
    public class BrandController : Controller
    {
        private readonly IBrandRepository _brandRepository;

        public BrandController(
            IBrandRepository brandRepository
            )
        {
            _brandRepository = brandRepository;
        }

        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Brand> brands;

            // Get all Brands from the company:
            brands = await _brandRepository.GetAllBrandsAsync();

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Brand);

            return View(brands);
        }
    }
}
