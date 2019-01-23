using PaymentGateway.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Models.Tags
{
    public class TagMerchant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Description { get; set; }

        public List<TagMerchantConnection> TagMerchantConnections { get; set; }
    }
}
