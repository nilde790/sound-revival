using System.ComponentModel.DataAnnotations;
using SoundRevival.Dto.Validation;

namespace SoundRevival.Dto.Listings;

public class UpdateListingRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)]
    [MaxDecimalPlaces(2)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Condition { get; set; } = string.Empty;

}
