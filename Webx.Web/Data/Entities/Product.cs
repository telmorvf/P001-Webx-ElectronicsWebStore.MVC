using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using static System.Net.WebRequestMethods;


namespace Webx.Web.Data.Entities
{
    public class Product : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        
        [Required]
        //[DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }
        
        [Required]
        public Brand Brand { get; set; }
        public int BrandId { get; set; }


        [Required]
        public Category Category { get; set; }
        public int CategoryId { get; set; }
   

        // ToDo Teste Telmo
        [ForeignKey("ProductId")]
        public ICollection<ProductImages> Images { get; set; }
        //public IEnumerable<ProductImages> Images { get; set; }

        public IEnumerable<ProductImages> Images { get; set; }

        public string Image => Images.Count() > 0 ? Images.ElementAt(0).ImageFullPath : "https://webx2022.blob.core.windows.net/images/NoPhoto.jpg";      
        
        [Required]
        [Display(Name = "Promotion?")]
        public bool IsPromotion { get; set; }


        [Required]
        [Display(Name = "Is Service?")]
        public bool IsService { get; set; }

        public string ImageFirst 
        {
            get
            {
                if (IsService == true)
                { return "https://webx2022.blob.core.windows.net/images/services.jpg"; }
                else
                {
                    if (Images == null)
                        return "https://webx2022.blob.core.windows.net/images/NoPhoto.jpg";
                    //else if (Images.Any())
                    else if (Images.Any())
                        return Images.ElementAt(0).ImageFullPath;
                    else
                        return "https://webx2022.blob.core.windows.net/images/NoPhoto.jpg";
                }
            }        
        }

        //public string ImageFullPath => ImageFirst == null
        //   ? $"https://webx2022.blob.core.windows.net/images/NoPhoto-round.jpg"
        //   : ImageFirst;

    }
}
