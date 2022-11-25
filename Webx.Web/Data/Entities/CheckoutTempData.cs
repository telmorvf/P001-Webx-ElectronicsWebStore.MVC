using PayPalCheckoutSdk.Orders;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class CheckoutTempData : IEntity
    {
        //ShippingData - pode ser diferente dos dados do user
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }

        public string NIF { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }



    }
}
