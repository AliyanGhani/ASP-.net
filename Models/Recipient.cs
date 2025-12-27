using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Admin.Models
{
    public class Recipient
    {
        [Key]
        public int RecipientId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Recipient Name")]
        [Column(TypeName = "Varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Delivery Address")]
        [Column(TypeName = "Varchar(255)")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits")]
        public long PhoneNo { get; set; }

        [Required(ErrorMessage = "Delivery date is required")]
        [Display(Name = "Delivery Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Delivery Time")]
        [DataType(DataType.Time)]
        public TimeSpan DeliveryTime { get; set; }

        [Display(Name = "Custom Message")]
        [Column(TypeName = "Varchar(500)")]
        public string? CustomMessage { get; set; }

        // Foreign key
        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}