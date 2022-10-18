using System;

namespace Webx.Web.Data.Entities
{
    public class ProductImages : IEntity
    {
        public int Id { get; set; }

        public Guid ImageId { get; set; }

    }
}
