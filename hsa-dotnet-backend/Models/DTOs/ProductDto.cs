using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HsaDotnetBackend.Models.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsHsa { get; set; }
    }
}