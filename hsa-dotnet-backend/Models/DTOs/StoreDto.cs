namespace HsaDotnetBackend.Models.DTOs
{
    public class StoreDto
    {
        public int StoreId { get; set; }
        public string Name { get; set; }
        public LocationDto Location { get; set; }
    }
}
