using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.ComponentModel.DataAnnotations;

namespace Order.Repository.Entities
{
    public class OrderDetail
    {
        [Key]
        public int id { get; set; }
        public string? forge_order_id { get; set; }
        public string? stripe_transaction_id { get; set; }
        public string bc_customer_id { get; set; }
        public DateTime order_date { get; set; }
        public decimal total_amount { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public string bc_status { get; set; }
        public string bc_order_id { get; set; }
        public string bc_order_no { get; set; }
        public string? BasketBarcode { get; set; }
        public string? BinBarcode { get; set; }
        public string? InvoiceID { get; set; }
        public string? CustomerName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? Postcode { get; set; }
        public string? State { get; set; }
        public string? Suburb { get; set; }
        public string? Country { get; set; }
        public string? RateID { get; set; }
        public string? CourierName { get; set; }
        public string? product_classification { get; set; }
        public bool? ColdChain { get; set; }
        public string? TrackingNo { get; set; }
        public string? PrescriberName { get; set; }
        public string? TrackingURL { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? ShippingLabel { get; set; }
        public Nullable<int> PackUserID { get; set; }
        public Nullable<int> PickUserID { get; set; }
        public string? PrescriberNo { get; set; }
        public string? PrescriberTitle { get; set; }
        public string? PrescriberPractiseAddress { get; set; }
        public string? PrescriberProfessionalQualitifcation { get; set; }
        public string? PrescriberPhoneNumber { get; set; }
        public DateTime? PrescriptionWritingDate { get; set; }
        public string? MedicationNumberOfRepeats { get; set; }
        public string? PrescriberSignature { get; set; }
        public string? PrescriberSignaturePath { get; set; }

        public virtual ICollection<OrderItemDetail> OrderItemDetails { get; set; }
    }
}
