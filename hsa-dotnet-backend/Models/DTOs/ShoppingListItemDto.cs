using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HsaDotnetBackend.Models.DTOs
{
    public class ShoppingListItemDto
    {
        public int ShoppingListItemId { get; set; }
        public string ProductName { get; set; }
        public int? Quantity { get; set; }
        public bool? Checked { get; set; }
        public ProductDto Product { get; set; }
        public StoreDto Store { get; set; }
    }
}