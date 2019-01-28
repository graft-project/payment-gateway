namespace PaymentGateway.Models.TagTerminalViewModels
{
    public class TagTerminalViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserId { get; internal set; }
    }
}
