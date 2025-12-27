using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Admin.Models
{
    public class Products
    {
        [Key]
        public int Pro_id { get; set; }

        [Required]
        [Column(TypeName = "Varchar(100)")]
        public string Pro_name { get; set; } = string.Empty;

        // CHANGE: int se decimal karein
        [Required]
        [Column(TypeName = "decimal(18,2)")] // Add this line
        public decimal Pro_price { get; set; }  // Changed from int to decimal

        [Column(TypeName = "Varchar(500)")]
        public string Pro_des { get; set; } = string.Empty;

        [Column(TypeName = "Varchar(50)")]
        public string Pro_category { get; set; } = string.Empty;

        [Column(TypeName = "Varchar(255)")]
        public string? Pro_imagename { get; set; }

        [NotMapped]
        public IFormFile? Pro_img { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}