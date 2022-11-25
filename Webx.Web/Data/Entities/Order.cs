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
        //[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public int InvoiceId { get; set; }

        public int TotalQuantity { get; set; }
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalPrice { get; set; }
        
        public Status Status { get; set; }

    }
}
