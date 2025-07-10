using System.ComponentModel.DataAnnotations;

namespace Order.Repository.Entities
{
    public class OrderItemDetail
    {
        [Key]
        public int id { get; set; }

        public int orderid { get; set; }
        public string bc_order_item_id { get; set; }
        public string bc_order_id { get; set; }
        public string bc_product_id { get; set; }
        public string product_id { get; set; }
        public string variant_id { get; set; }
        public string name { get; set; }
        public int quantity { get; set; }
        public int scanned_quantity { get; set; }
        public decimal unit_price { get; set; }
        //public decimal unit_cost { get; set; }
        public decimal total_price { get; set; }
        public decimal vat_rate { get; set; }
        public string? product_barcode { get; set; }
        public string? dosage_label { get; set; }
        public Nullable<int> scanned_dosage_label { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public Nullable<int> PackUserID { get; set; }

        public virtual OrderDetail OrderDetail { get; set; }
    }
}
