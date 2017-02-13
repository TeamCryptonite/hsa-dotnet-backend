using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HsaDotnetBackend.Models.DTOs
{
    public class LineItemDto
    {
        public int Id { get; set;}
        public int ReceiptId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public ProductDto Product { get; set; }
    }
}