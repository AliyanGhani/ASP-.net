namespace Admin.Models
{
    public class CheckoutModel
    {
        public Cart? Cart { get; set; }
        public Recipient Recipient { get; set; } = new Recipient();
        public string? CustomMessage { get; set; }
        public string PaymentMethod { get; set; } = "Credit Card";
    }
}