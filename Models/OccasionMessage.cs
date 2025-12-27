using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Admin.Models
{
    public class OccasionMessage
    {
        [Key]
        public int Occasionid { get; set; }

        [Required]
        [Column(TypeName = "Varchar(100)")]
        public string OccasionName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "Varchar(500)")]
        public string Messages { get; set; } = string.Empty;
    }
}