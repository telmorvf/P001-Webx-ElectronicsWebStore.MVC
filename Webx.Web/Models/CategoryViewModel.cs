using Microsoft.AspNetCore.Http;
using Webx.Web.Data.Entities;

namespace Webx.Web.Models
{
    public class CategoryViewModel : Category
    {
        public IFormFile PictureFile { get; set; }
    }
}
