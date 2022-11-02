using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Base;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;

namespace Webx.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IUserHelper _userHelper;

        public EmployeeController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }

        public async Task<IActionResult> ViewAll()
        {
            //vai buscar os empregados da empresa (Admins/technicians/product managers)
            var employees = await _userHelper.GetAllEmployeesAsync();
            //vai buscar as dataAnnotations da class User para injectar na tabela do syncfusion
            ViewBag.Type = typeof(User);

            return View(employees);
        }

       

       

    }
}
