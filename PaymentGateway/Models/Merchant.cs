using PaymentGateway.Data;
using PaymentGateway.Models.Tags;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Models
{
    public enum MerchantStatus : sbyte
    {
        Active = 0,
        Disabled = 1
    }

    public class Merchant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Address { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string WalletAddress { get; set; }

        [Required]
        public MerchantStatus Status { get; set; }


        public List<TagMerchantConnection> Tags { get; set; }

        // -----------------------------------
        public List<Store> Stores { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

    }
}
