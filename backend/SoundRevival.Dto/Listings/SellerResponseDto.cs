using System.Text.Json.Serialization;

namespace SoundRevival.Dto.Listings;

public class SellerResponseDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

}
