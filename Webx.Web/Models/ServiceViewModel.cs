using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.Spreadsheet;


namespace Webx.Web.Models
{
    public class ServiceViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "The field {0} can contain {1} characteres lenght")]
        public string Name { get; set; }

        public string Description { get; set; }

        // não usa este formato em modo de edição
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        [Range(0, 99999.99, ErrorMessage = "Only greater than Zero")]
        public decimal Price { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Tou Must Select Service = True")]
        public bool IsService { get; set; }

        [Display(Name = "Brand")]
        public string DefaultWebxBrandName => "WebX Service";
        //[Required]
        //[Display(Name = "Brand")]
        //[Range(6, 6, ErrorMessage = "You must select the brand: WebX")]
        //public string BrandId { get; set; }
        //public IEnumerable<SelectListItem> Brands { get; set; }


        [Required]
        [Display(Name = "Category")]
        [Range(1, 1, ErrorMessage = "You must select a category: Services")]
        public string CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }



        public string ImageFirst { get; set; }

        [Required]
        [Display(Name = "Discount(%)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Discount { get; set; }

        [Display(Name = "Price with Discount")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal PriceWithDiscount => Price * (1 - (Discount / 100));

    }
}
