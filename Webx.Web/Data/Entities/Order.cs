using System;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class Order : IEntity
    {
        public int Id { get; set; }

        [Required]
        public User Customer { get; set; }
        [Required]
        public Store Store { get; set; }

        public Appointment Appointment { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public int InvoiceId { get; set; }

        public int TotalQuantity { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
