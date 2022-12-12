using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace Webx.Web.Helpers
{
    public interface IImageHelper
    {
        Task<Guid> UploadImageAsync(IFormFile pictureFile, Guid modelImageId, string folder);
    }
}
