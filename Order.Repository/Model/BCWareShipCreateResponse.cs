using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Model
{
    public class BCWareShipCreateResponse
    {
        public string status { get; set; }
        public string message { get; set; }
        public string orderNo { get; set; }
        public string shipmentNo { get; set; }
        public string pickNo { get; set; }
        public string invoiceId { get; set; }
        public string locationCode { get; set; }
    }
}
