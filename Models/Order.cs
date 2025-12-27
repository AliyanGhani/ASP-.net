using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Admin.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Order Status")]
        [Column(TypeName = "Varchar(20)")]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Delivered, Cancelled

        [Display(Name = "Payment Status")]
        [Column(TypeName = "Varchar(20)")]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        [Display(Name = "Transaction ID")]
        [Column(TypeName = "Varchar(100)")]
        public string? TransactionId { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Recipient? Recipient { get; set; }
    }
}
