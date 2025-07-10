using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Entities
{
    public class AppConfigurationSetting
    {
        [Key]
        public int ID { get; set; }
        public string SystemName { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
