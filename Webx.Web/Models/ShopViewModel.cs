using X.PagedList;
using System.Collections.Generic;
using Webx.Web.Data.Entities;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Webx.Web.Models
{
    public class ShopViewModel
    {
        public IPagedList<Product> PagedListProduct { get; set; }

        public string SelectedCategory { get; set; }

        public List<string> BrandsTags { get; set; }

        public int NumberOfProductsFound { get; set; }             

        public int ResultsPerPage { get; set; }

        public List<Category> Categories { get; set; }

        public List<Brand> Brands { get; set; }

        public decimal MostExpensiveProductPrice { get; set; }
        
        public Product Product { get; set; }

        public List<CartViewModel> Cart { get; set; }

        public int TotalProductsInCart => Cart.Sum(p => p.Quantity);

        public string CartGrandTotal => Cart.Sum(p => p.Product.PriceWithDiscount * p.Quantity).ToString("C2");  
        
        public string CartGrandTotalWithNoDiscount => Cart.Sum(p => p.Product.Price * p.Quantity).ToString("C2");

        public bool CookieConsent { get; set; }

        public ChangeUserViewModel UserViewModel { get; set; }

        public CheckoutViewModel CheckoutViewModel { get; set; }

        public List<Stock> Stocks { get; set; }

        public IEnumerable<SelectListItem> Stores { get; set; }

        public IEnumerable<SelectListItem> PhysicalStores { get; set; }

        public List<InvoiceViewModel> Invoices { get; set; }

        public List<OrderWithDetailsViewModel> CustomerOrders { get; set; }

        public Order OrderToSchedule { get; set; }

        public bool HasAppointmentToDo { get; set; }

        public List<Product> SuggestedProducts { get; set; }

        public List<Product> HighlightedProducts { get; set; }

        public int CompletedOrders
        {
            get
            {
                int completedOrders = 0;
                if(CustomerOrders != null)
                {
                    if (CustomerOrders.Count() > 0)
                    {
                        foreach (var item in CustomerOrders)
                        {
                            if (item.Order.Status.Name == "Appointment Done" || item.Order.Status.Name == "Order Closed")
                            {
                                completedOrders++;
                            }
                        }
                    }
                }               

                return completedOrders;
            }
        }

        public int PendingOrders
        {
            get
            {
                int pendingOrders = 0;
                if(CustomerOrders != null)
                {
                    if (CustomerOrders.Count() > 0)
                    {
                        foreach (var item in CustomerOrders)
                        {
                            if (item.Order.Status.Name != "Appointment Done" && item.Order.Status.Name != "Order Closed")
                            {
                                pendingOrders++;
                            }
                        }
                    }
                }                

                return pendingOrders;
            }
        }
        
        public List<Product> WishList { get; set; }

        public bool GoToWishList { get; set; } = false;

    }
}
