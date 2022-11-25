namespace Webx.Web.Data.Entities
{
    public class Status: IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
