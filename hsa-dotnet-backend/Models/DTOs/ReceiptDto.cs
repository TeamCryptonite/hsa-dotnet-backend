﻿using System;
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
        public int StoreId { get; set; }
        public DateTime? DateTime { get; set; }
        public bool? IsScanned { get; set; }
        public string PictureId { get; set; }
        public string PictureUrl => ReceiptPictureHelper.GetReceiptPictureUrl(PictureId);
        public ICollection<LineItemDto> LineItems { get; set; }
    }
}