using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using AspNetCoreHero.ToastNotification.Abstractions;
using Webx.Web.Data.Repositories;
using Webx.Web.Data;

namespace Webx.Web.Helpers
{
    public class ImageHelper : IImageHelper
    {
        private readonly IBlobHelper _blobHelper;

        public ImageHelper(
            IBlobHelper blobHelper
            )
        {
            _blobHelper = blobHelper;
        }


        public async Task<Guid> UploadImageAsync(IFormFile pictureFile, Guid modelImageId, string folder)
        {
            Guid imageId = modelImageId;

            using var image = Image.Load(pictureFile.OpenReadStream());
            image.Mutate(img => img.Resize(512, 0));

            using (MemoryStream m = new MemoryStream())
            {
                image.SaveAsJpeg(m);
                byte[] imageBytes = m.ToArray();
                imageId = await _blobHelper.UploadBlobAsync(imageBytes, folder);
            }
            return await Task.FromResult(imageId);
        }
    }
}
