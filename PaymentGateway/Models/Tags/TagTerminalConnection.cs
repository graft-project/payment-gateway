using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Models.Tags
{
    public class TagTerminalConnection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TagId { get; set; }

        [Required]
        public int TerminalId { get; set; }

        [ForeignKey("TagId")]
        public TagTerminal Tag { get; set; }

        [ForeignKey("TerminalId")]
        public Terminal Terminal { get; set; }
    }
}
