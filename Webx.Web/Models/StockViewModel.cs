using Microsoft.AspNetCore.Http;
using Syncfusion.EJ2.Spreadsheet;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class StockViewModel : Stock
    {
        public IFormFile PictureFile { get; set; }


        public string ImageFirst
        {
            get
            {
                { return Product.ImageFirst ; }
            }
        }

    }
}

