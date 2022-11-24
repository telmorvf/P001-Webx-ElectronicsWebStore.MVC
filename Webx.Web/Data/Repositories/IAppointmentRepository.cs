using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<string> GetAllEventsCustomerCanSeeAsync();
    }
}
