using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HsaDotnetBackend.Models.DTOs
{
    public class ReceiptDto
    {
        public ReceiptDto()
        {
            this.LineItems = new List<LineItemDto>();
        }
        public int Id { get; set; }
        public int? UserObjectId { get; set; }
        public int StoreId { get; set; }
        public DateTime? DateTime { get; set; }
        public bool? IsScanned { get; set; }
        public ICollection<LineItemDto> LineItems { get; set; }
    }
}