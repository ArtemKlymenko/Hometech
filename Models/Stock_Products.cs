namespace Hometech.Models
{
    public class stock_products
    {
        public string vendor_code { get; set; }
        public int id_manufacturer { get; set; }
        public int id_category { get; set; }
        public decimal price { get; set; }
        public int amount { get; set; }
    }
}