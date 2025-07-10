using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Model
{
    public class BCCustomerListResponse
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        public List<BCCustomerResponse> value { get; set; }
    }
}
