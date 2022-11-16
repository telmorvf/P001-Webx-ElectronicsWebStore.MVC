using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.Spreadsheet;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        //[Required]
        //[Display(Name = "Price")]
        //[MinLength(0, ErrorMessage = "Only greater than Zero")]
        public decimal Price { get; set; }

        public bool IsService { get; set; }


        //[Required]
        [Display(Name = "Brand")]
        [MinLength(1, ErrorMessage = "Must select a brand.")]
        public string BrandId { get; set; }


        public IEnumerable<SelectListItem> Brands { get; set; }


        [Required]
        [Display(Name = "Category")]
        [MinLength(1, ErrorMessage = "Must select a category.")]
        public string CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        
        //[Required]
        [Display(Name = "Minimum Quantity Alert?")]
        //[Range(1, int.MaxValue, ErrorMessage = "Only greater than Zero")]
        public int MinimumQuantity { get; set; }

        [Display(Name = "Picture File")]
        public IFormFile PictureFile { get; set; }

        public IEnumerable<ProductImages> Images { get; set; }
        public Images ImagesId { get; set; }

        public string ImageFirst { get; set; }

    }
}
