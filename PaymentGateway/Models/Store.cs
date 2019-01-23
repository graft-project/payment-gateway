using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Models
{
    public enum StoreStatus : sbyte
    {
        Active = 0,
        Disabled = 1
    }

    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MerchantId { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Address { get; set; }

        [Required]
        [Column("Status")]
        public StoreStatus Status { get; set; }

        // --------------------------------------
        public Merchant Merchant { get; set; }
    }
}
