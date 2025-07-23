using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Entities
{
    public class PrescriptionDownloadRequest
    {
        [Key]
        public int ID { get; set; }
        public string OrderIds { get; set; }
        public string ForgeOrderIds { get; set; }
        public string? BlobFileName { get; set; }
        public string? BlobFilePath { get; set; }
        public bool IsSent { get; set; }
        public bool IsWorkDone { get; set; }
        public int TryCount { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
