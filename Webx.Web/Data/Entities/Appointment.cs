using System;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class Appointment : IEntity
    {
        public int Id { get; set; }

        public User WorkerID { get; set; }

        public DateTime AppointmentDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime BegginingHour { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndHour { get; set; }

        public string Comments { get; set; }

        public bool HasAttended { get; set; }

    }
}
