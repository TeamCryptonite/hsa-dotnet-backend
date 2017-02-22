namespace HsaDotnetBackend.Models.DTOs
{
    public class LineItemDto
    {
        public int LineItemId { get; set;}
        public int ReceiptId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsHsa { get; set; }
        public ProductDto Product { get; set; }
    }
}