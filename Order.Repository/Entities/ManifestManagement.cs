using System.ComponentModel.DataAnnotations;

namespace Order.Repository.Entities
{
    public class ManifestManagement
    {
        [Key]
        public int ID { get; set; }
        public string ManifestID { get; set; }
        public string ManifestURL { get; set; }
        public string GroupID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
