namespace SoundRevival.Dto.Listings;

public class ListingDetailResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public required SellerResponseDto Seller { get; set; }
    public List<ListingImageResponseDto> Images { get; set; } = [];

}
