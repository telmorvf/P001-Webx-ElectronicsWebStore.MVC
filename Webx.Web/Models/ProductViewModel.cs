using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class ProductViewModel
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

        public bool IsService { get; set; }


        [Required]
        [Display(Name = "Brand")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a brand.")]
        public string BrandId { get; set; }
        public string BrandName { get; set; }
        public IEnumerable<SelectListItem> Brands { get; set; }

        [Required]
        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a category.")]
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }


        //[Display(Name = "Minimum Quantity Alert?")]

        //[Range(1, int.MaxValue, ErrorMessage = "Only greater than Zero")]
        //public int MinimumQuantity { get; set; }

        //[Display(Name = "Quantity Received?")]
        //[Range(0, int.MaxValue, ErrorMessage = "Only Zero or greater than Zero")]
        //public int ReceivedQuantity { get; set; }


        [Display(Name = "Picture File")]
        public IFormFile PictureFile { get; set; }

        [Display(Name = "Upload Files")]
        public IList<IFormFile> UploadFiles { get; set; }

        // ToDo Telmo
        public ICollection<ProductImages> Images { get; set; }
        public Product Product { get; set; }
        //public List<ProductImages> product_Images { get; set; }
        public Images ImagesId { get; set; }

        public string ImageFirst { get; set; }


    }
}
