using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class CheckoutViewModel
    {
        public User User { get; set; }

        [Display(Name ="Shipping Address")]
        [Required(ErrorMessage = "A shipping address is mandatory to finish the Order.")]
        [MaxLength(100, ErrorMessage = "The field Address cannot have more then 100 characters.")]
        public string ShippingAddress { get; set; }
                
    }
}
