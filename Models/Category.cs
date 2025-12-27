using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Admin.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "Varchar(100)")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Column(TypeName = "Varchar(500)")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "Varchar(255)")]
        [Display(Name = "Image Name")]
        public string ImageName { get; set; } = "default-category.jpg";

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [NotMapped]
        [Display(Name = "Category Image")]
        public IFormFile? CategoryImage { get; set; }

        // YEH LINE ADD KAREIN
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<Products> Products { get; set; } = new List<Products>();
    }
}