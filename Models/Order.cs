namespace Hometech.Models
{
    public class Order
    {
        public int id_order { get; set; }
        public int id_client { get; set; }
        public string status { get; set; }
        public decimal total_price { get; set; } = 0;
    }
}