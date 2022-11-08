using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Data.Repositories;

namespace Webx.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController( 
            ICategoryRepository categoryRepository
            )
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> ViewAll()
        {
            IEnumerable<Category> categories;

            // Get all Brands from the company:
            categories = await _categoryRepository.GetAllCategoriesAsync();

            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(Brand);

            return View(categories);
        }
    }
}
