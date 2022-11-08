using System;
using System.ComponentModel.DataAnnotations;

namespace Webx.Web.Data.Entities
{
    public class Brand : IEntity
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Guid ImageId { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
           ? $"https://webx2022.blob.core.windows.net/images/NoPhoto-round.jpg"
           : $"https://webx2022.blob.core.windows.net/brands/{ImageId}";
    }
}
