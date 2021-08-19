using System;
namespace Hometech.Models
{
    public class Delivery
    {
        public int id_order{ get; set; }
        public int id_courier{ get; set; }
        public DateTime order_datetime{ get; set; }
        public DateTime? delivery_datetime{ get; set; }
    }
}