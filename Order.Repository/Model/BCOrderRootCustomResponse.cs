using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Model
{
    public class BCOrderRootCustomResponse
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        public string value { get; set; }
    }
}
