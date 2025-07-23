using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Model
{
    public class PrescriptionDetailResponse
    {
        public string? prescriber_title { get; set; }
        public string? prescriber_name { get; set; }
        public string? prescriber_number { get; set; }
        public string? prescriber_practise_address { get; set; }
        public string? prescriber_professional_qualitifcation { get; set; }
        public string? prescriber_phone_number { get; set; }
        public DateTime? prescription_writing_date { get; set; }
        public string? medication_number_of_repeats { get; set; }
        public string? prescriber_signature_file { get; set; }
        public string? prescriber_signature_folder { get; set; }
    }

    public class PrescriptionDetailErrorResponse
    {
        public string error { get; set; }
    }
}
