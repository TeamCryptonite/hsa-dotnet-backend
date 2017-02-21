namespace HsaDotnetBackend.Models.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsHsa { get; set; }
        public string ImageId { get; set; }
    }
}