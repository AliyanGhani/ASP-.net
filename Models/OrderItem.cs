using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Products? Product { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
