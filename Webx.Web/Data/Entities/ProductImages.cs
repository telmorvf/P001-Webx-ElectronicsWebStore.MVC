using System;

namespace Webx.Web.Data.Entities
{
    public class ProductImages : IEntity
    {
        public int Id { get; set; }

        public Guid ImageId { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
        ? $"https://webx2022.blob.core.windows.net/images/NoPhoto.jpg"
        : $"https://webx2022.blob.core.windows.net/products/{ImageId}";

    }
}
