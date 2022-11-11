using Newtonsoft.Json;
using System.Collections.Generic;

namespace Webx.Web.Models
{
    public class FiltersStringModel
    {
        [JsonProperty("filters")]
        public List<string> Filters { get; set; }

        [JsonProperty("filtersNames")]
        public List<string> FiltersNames { get; set; }
    }
}
