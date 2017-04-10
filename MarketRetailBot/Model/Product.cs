using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MarketRetailBot.Model
{
    public class Product
    {
        public Product() { }

        public string ColorWay { get; set; }
        public string Title { get; set; }
        public string Sku { get; set; }
        public string ThumbUrl { get; set; }
        public string MainImageUrl  { get; set; }
    }
}