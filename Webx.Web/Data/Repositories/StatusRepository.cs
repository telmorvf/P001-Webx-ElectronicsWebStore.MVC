using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;
namespace Webx.Web.Data.Repositories
{
    public class StatusRepository : GenericRepository<Status>, IStatusRepository
    {
        private readonly DataContext _context;

        public StatusRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Status> GetStatusByIdAsync(string id)
        {
            var status = await _context.Statuses.Where(s => s.Id.ToString() == $"{id}").FirstOrDefaultAsync();

            return status;
        }

        public IEnumerable<SelectListItem> GetStatusesCombo()
        {
            var list = new List<Status>();
            var comboList = new List<SelectListItem>();

            list.Add(_context.Statuses.Where(s => s.Name == "Order Closed").FirstOrDefault());
            list.Add(_context.Statuses.Where(s => s.Name == "Appointment Done").FirstOrDefault());
            list.Add(_context.Statuses.Where(s => s.Name == "Order Shipped").FirstOrDefault());
            list.Add(_context.Statuses.Where(s => s.Name == "Appointment Created").FirstOrDefault());
            list.Add(_context.Statuses.Where(s => s.Name == "Order Created").FirstOrDefault());
            list.Add(_context.Statuses.Where(s => s.Name == "Pending Appointment").FirstOrDefault());

            foreach (var status in list)
            {
                comboList.Add(new SelectListItem
                {
                    Text = status.Name,
                    Value = status.Id.ToString(),
                });

            }

            return comboList.OrderBy(l => l.Text);
        }
    }
}
