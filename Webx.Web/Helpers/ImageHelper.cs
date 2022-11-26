using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;




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
