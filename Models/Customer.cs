using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Admin.Models
{
    public class Customer
    {
        [Key]
        public int Custid { get; set; }

        [Required]
        [Column(TypeName = "Varchar(50)")]
        public string F_name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "Varchar(50)")]
        public string L_name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Dob { get; set; }

        [Required]
        [Column(TypeName = "Char(1)")]
        public char Gender { get; set; }

        [Required]
        public long P_no { get; set; }

        [Required]
        [Column(TypeName = "Varchar(255)")]
        public string Address { get; set; } = string.Empty;
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        // COMMENT: Temporarily remove navigation properties
        // public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}