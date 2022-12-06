using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Webx.Web.Data.Repositories;

namespace Webx.Web.Controllers
{
    public class AdminPanelController : Controller
    {
        private readonly IProductRepository _productRepository;

        public AdminPanelController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TempsCounter = await _productRepository.GetReviewsTempsCountAsync();

            

            return View();
        }
    }
}
