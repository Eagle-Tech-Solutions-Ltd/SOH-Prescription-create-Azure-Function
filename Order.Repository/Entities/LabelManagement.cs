using System.ComponentModel.DataAnnotations;

namespace Order.Repository.Entities
{
    public class LabelManagement
    {
        [Key]
        public int ID { get; set; }
        public string LabelID { get; set; }
        public string ShipmentID { get; set; }
        public int OrderID { get; set; }
        public string? ForgeOrderID { get; set; }
        public string? GroupID { get; set; }
        public string? LabelPath { get; set; }
        public bool IsManifested { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
