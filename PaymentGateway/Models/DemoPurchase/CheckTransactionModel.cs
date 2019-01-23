using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Models
{
    public class CheckTransactionModel
    {
        public string PaymentId { get; set; }
        public string Address { get; set; }
    }
}
