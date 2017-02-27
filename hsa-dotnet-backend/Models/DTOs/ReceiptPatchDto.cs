using System;

namespace HsaDotnetBackend.Models.DTOs
{
    public class ReceiptPatchDto
    {
        public int ReceiptId { get; set; }
        public StoreDto Store { get; set; }
        public DateTime? DateTime { get; set; }
        public bool? IsScanned { get; set; }
        public string ImageId { get; set; }
    }
}