using Order.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Model
{
    public class PendingPrescriptionResponse
    {
        public List<OrderDetail> orders { get; set; }
        public int PrescriptionRequestID { get; set; }
    }
}
