using PaymentGateway.Models.Tags;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Models
{
    public enum TerminalStatus : sbyte
    {
        Active = 0,
        Disabled = 1,
        DisabledByServiceProvider = 2
    }

    public class Terminal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StoreId { get; set; }

        [Required]
        public int MerchantId { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string SerialNumber { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Required]
        public TerminalStatus Status { get; set; }

        public bool Virtual { get; set; }

        public string ApiSecret { get; set; }

        public List<TagTerminalConnection> Tags { get; set; }

        // --------------------------------------
        [ForeignKey("ServiceProviderId")]
        public ServiceProvider ServiceProvider { get; set; }

        [ForeignKey("StoreId")]
        public Store Store { get; set; }

        [ForeignKey("MerchantId")]
        public Merchant Merchant { get; set; }
    }
}
