using PaymentGateway.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Models
{
    public enum ServiceProviderStatus : sbyte
    {
        Active = 0,
        Disabled = 1
    }

    public class ServiceProvider
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }

        [Required]
        public string UserId { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Address { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string WalletAddress { get; set; }

        [Required]
        public float TransactionFee { get; set; }

        [Required]
        public ServiceProviderStatus Status { get; set; }


        // -----------------------------------
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
