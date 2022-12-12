using System;

namespace Webx.Web.Data.Entities
{
    public class StatusChecker : IEntity
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }
        
    }
}
