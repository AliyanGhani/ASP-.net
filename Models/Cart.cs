using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
