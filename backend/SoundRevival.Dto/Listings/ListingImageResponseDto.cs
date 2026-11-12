namespace SoundRevival.Dto.Listings
{
    public class ListingImageResponseDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public short DisplayOrder { get; set; }

    }
}
