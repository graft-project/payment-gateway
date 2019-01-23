using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Models.Tags
{
    public class TagMerchantConnection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TagId { get; set; }

        [Required]
        public int MerchantId { get; set; }

        [ForeignKey("TagId")]
        public TagMerchant Tag { get; set; }

        [ForeignKey("MerchantId")]
        public Merchant Merchant { get; set; }
    }
}
