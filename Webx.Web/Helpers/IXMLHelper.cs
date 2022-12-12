using System.Collections.Generic;
using Webx.Web.Data.Entities;

namespace Webx.Web.Helpers
{
    public interface IXMLHelper
    {
        string GenerateXML(List<Stock> products, string user);
    }
}
