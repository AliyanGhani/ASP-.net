using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        public int ProductId { get; set; }
        public Products? Product { get; set; }

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;

        public decimal Price { get; set; }
    }
}
