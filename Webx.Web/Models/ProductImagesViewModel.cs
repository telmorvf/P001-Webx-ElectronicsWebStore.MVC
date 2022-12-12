using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Metadata;
using Syncfusion.EJ2.Spreadsheet;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class ProductImagesViewModel
    {
        public ICollection<ProductImages> Images { get; set; }
        public Product Product { get; set; }
    }
}
