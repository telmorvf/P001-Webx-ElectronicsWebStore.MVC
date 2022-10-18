using System;

namespace Webx.Web.Data.Entities
{
    public class Appointment : IEntity
    {
        public int Id { get; set; }

        public User WorkerID { get; set; }

        public DateTime AppointmentDate { get; set; }

        public DateTime BegginingHour { get; set; }

        public DateTime EndHour { get; set; }

        public string Comments { get; set; }

        public bool HasAttended { get; set; }

    }
}
