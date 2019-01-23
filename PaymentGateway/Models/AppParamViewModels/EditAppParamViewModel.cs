using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models.AppParamViewModels
{
    public class EditAppParamViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
