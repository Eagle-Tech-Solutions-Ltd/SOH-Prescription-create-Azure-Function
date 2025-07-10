using System.ComponentModel.DataAnnotations;

namespace Order.Repository.Entities
{
    public class ActivityLog
    {
        [Key]
        public int ID { get; set; }
        public string FromRequest { get; set; }
        public string ToResponse { get; set; }
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
