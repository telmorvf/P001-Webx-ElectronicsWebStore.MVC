using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Webx.Web.Models
{
    public class AppointmentViewModel
    {
        public IEnumerable<SelectListItem> PhysicalStoresCombo { get; set; }

        public int StoreId { get; set; }

        public int AppointmentsCreatedTotals { get; set; }

        public int OngoingAppointmentsTotals { get; set; }

        public int AppointmentsDoneTotals { get; set; }
    }
}
