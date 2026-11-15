using SoundRevival.Dto.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoundRevival.Dto.Listings
{
    public class ListingFilterRequestDto : IValidatableObject
    {
        [StringLength(150)]
        public string? Category {  get; set; }

        [StringLength(150)]
        public string? Condition { get; set; }

        [Range(typeof(decimal), "0", "1000000", ParseLimitsInInvariantCulture = true)]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0", "1000000", ParseLimitsInInvariantCulture = true)]
        public decimal? MaxPrice { get; set; }

        [StringLength(100)]
        public string? Search { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = ListingValues.Available;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinPrice > MaxPrice && MinPrice is not null && MaxPrice is not null)
            {
                yield return new ValidationResult("the minimun price can't be bigger than the maximum price", [nameof(MaxPrice), nameof(MinPrice)]);
            }
        }
    }
}
