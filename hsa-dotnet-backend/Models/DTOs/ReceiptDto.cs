using System;
using System.Collections.Generic;
using HsaDotnetBackend.Helpers;

namespace HsaDotnetBackend.Models.DTOs
{
    public class ReceiptDto
    {
        public ReceiptDto()
        {
            this.LineItems = new List<LineItemDto>();
        }
        public int ReceiptId { get; set; }
        public StoreDto Store { get; set; }
        public DateTime? DateTime { get; set; }
        public bool? IsScanned { get; set; }
        public string ImageId { get; set; }
        public string ImageUrl => ReceiptPictureHelper.GetReceiptImageUrl(ImageId);
        public ICollection<LineItemDto> LineItems { get; set; }
    }
}