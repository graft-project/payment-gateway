namespace PaymentGateway.Models.TagMerchantViewModels
{
    public class TagMerchantViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserId { get; internal set; }
    }
}
