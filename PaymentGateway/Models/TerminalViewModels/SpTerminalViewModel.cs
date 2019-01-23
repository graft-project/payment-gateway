using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.TerminalViewModels
{
    public class SpTerminalViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Merchant")]
        public int MerchantId { get; set; }

        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Terminal Name")]
        public string Name { get; set; }

        [Display(Name = "Status")]
        public TerminalStatus Status { get; set; }
    }
}
