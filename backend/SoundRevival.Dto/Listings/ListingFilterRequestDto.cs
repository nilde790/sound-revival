using System.ComponentModel.DataAnnotations;
using SoundRevival.Dto.Constants;


namespace SoundRevival.Dto.Listings;

public class ListingFilterRequestDto : IValidatableObject
{
    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(50)]
    public string? Condition { get; set; }

    [Range(typeof(decimal), "0", "1000000", ParseLimitsInInvariantCulture = true)]
    public decimal? MinPrice { get; set; }

    [Range(typeof(decimal), "0", "1000000", ParseLimitsInInvariantCulture = true)]
    public decimal? MaxPrice { get; set; }

    [StringLength(100)]
    public string? Search { get; set; }

    [StringLength(50)]
    [Required]
    public string Status { get; set; } = ListingValues.Available;

    [StringLength(20)]
    [Required]
    public string Sort { get; set; } = ListingValues.SortNewest;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice > MaxPrice)
        {
            yield return new ValidationResult("the minimum price can't be bigger than the maximum price", [nameof(MaxPrice), nameof(MinPrice)]);
        }
    }
}
