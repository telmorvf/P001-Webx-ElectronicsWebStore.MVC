using Microsoft.AspNetCore.Http;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class BrandViewModel : Brand
    {
        public IFormFile PictureFile { get; set; }


    }
}
