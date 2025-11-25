namespace Acme.SimpleTaskApp.Payment.VNPay
{
    public class VnpayConfig
    {
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string BaseUrl { get; set; }
    }
}
