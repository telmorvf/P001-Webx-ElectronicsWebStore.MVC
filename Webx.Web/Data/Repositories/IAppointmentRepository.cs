using System.Threading.Tasks;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        Task<string> GetAllEventsCustomerCanSeeAsync(int storeId);
        Task<string> GetAllEventsAsync();
        Task<string> GetSpecificStoreEvents(int storeId);
        Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
    }
}
