using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MarketRetailBot.Model
{
    public class ProductAvailability
    {
        public ProductAvailability() { }

        public string nid { get; set; }
        public string sku { get; set; }
        public decimal size { get; set; }
        public decimal total_price { get; set; }
       
    }
}