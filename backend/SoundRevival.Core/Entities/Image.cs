namespace SoundRevival.Core.Entities;

public class Image
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Url { get; set; } = string.Empty;
    public short DisplayOrder { get; set; }

    public Listing Listing { get; set; } = null!;
}
