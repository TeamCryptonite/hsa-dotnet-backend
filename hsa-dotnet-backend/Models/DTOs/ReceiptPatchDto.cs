using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HsaDotnetBackend.Helpers;

namespace HsaDotnetBackend.Models.DTOs
{
    public class ReceiptPatchDto
    {
        public int ReceiptId { get; set; }
        public int StoreId { get; set; }
        public DateTime? DateTime { get; set; }
        public bool? IsScanned { get; set; }
        public string PictureId { get; set; }
    }
}