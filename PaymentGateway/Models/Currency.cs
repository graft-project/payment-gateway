using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Models
{
    public class Currency
    {
        [Key]
        [Column(TypeName = "char(3)")]
        public string Code { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
    }
}
