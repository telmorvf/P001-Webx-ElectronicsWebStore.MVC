using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Webx.Web.Data.Repositories;
using Microsoft.AspNetCore.Http;

namespace Webx.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var results = await _productRepository.GetProductsAsync();
            var result = Ok(results);
            return result;
        }
    }
}
