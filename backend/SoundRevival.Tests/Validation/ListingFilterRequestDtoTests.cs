using SoundRevival.Dto.Constants;
using SoundRevival.Dto.Listings;

namespace SoundRevival.Tests.Validation
{
    public class ListingFilterRequestDtoTests
    {
        [Fact]
        public void Validate_MinPriceGreaterThanMaxPrice_ReturnsErrorOnBothPrices()
        {
            var request = new ListingFilterRequestDto();
            request.MinPrice = 100m;
            request.MaxPrice = 50m;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.MinPrice), error.MemberNames);
            Assert.Contains(nameof(ListingFilterRequestDto.MaxPrice), error.MemberNames);
        }

        [Fact]
        public void Validate_MinPriceOutOfRangeAndGreaterThanMax_ReturnsOnlyRangeError()
        {
            var request = new ListingFilterRequestDto();
            request.MinPrice = 2_000_000m;
            request.MaxPrice = 50m;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.MinPrice), error.MemberNames);
            Assert.DoesNotContain(nameof(ListingFilterRequestDto.MaxPrice), error.MemberNames);

        }

        [Fact]
        public void Validate_DefaultFilter_ReturnsNoErrors()
        {
            var request = new ListingFilterRequestDto();

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        // ---------- Price range ----------

        // Inclusive range, and each bound is optional.
        // TheoryData, not [InlineData]: attribute arguments can't be decimal.
        public static TheoryData<decimal?, decimal?> ValidPriceRanges => new()
        {
            { 100m, 100m },
            { 0m, 1000000m },
            { 100m, null },
            { null, 100m }
        };

        [Theory]
        [MemberData(nameof(ValidPriceRanges))]
        public void Validate_ValidPriceRange_ReturnsNoErrors(decimal? minPrice, decimal? maxPrice)
        {
            var request = new ListingFilterRequestDto
            {
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_NegativeMinPrice_ReturnsErrorOnMinPrice()
        {
            var request = new ListingFilterRequestDto { MinPrice = -1m };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.MinPrice), error.MemberNames);
        }

        [Fact]
        public void Validate_MaxPriceTooHigh_ReturnsErrorOnMaxPrice()
        {
            var request = new ListingFilterRequestDto { MaxPrice = 1000000.01m };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.MaxPrice), error.MemberNames);
        }

        // ---------- String lengths ----------

        [Fact]
        public void Validate_StringsAtMaxLength_ReturnsNoErrors()
        {
            var request = new ListingFilterRequestDto
            {
                Category = new string('a', 50),
                Condition = new string('a', 50),
                Search = new string('a', 100),
                Status = new string('a', 50),
                Sort = new string('a', 20)
            };

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_CategoryTooLong_ReturnsErrorOnCategory()
        {
            var request = new ListingFilterRequestDto { Category = new string('a', 51) };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.Category), error.MemberNames);
        }

        [Fact]
        public void Validate_ConditionTooLong_ReturnsErrorOnCondition()
        {
            var request = new ListingFilterRequestDto { Condition = new string('a', 51) };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.Condition), error.MemberNames);
        }

        [Fact]
        public void Validate_SearchTooLong_ReturnsErrorOnSearch()
        {
            var request = new ListingFilterRequestDto { Search = new string('a', 101) };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.Search), error.MemberNames);
        }

        [Fact]
        public void Validate_StatusTooLong_ReturnsErrorOnStatus()
        {
            var request = new ListingFilterRequestDto { Status = new string('a', 51) };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.Status), error.MemberNames);
        }

        [Fact]
        public void Validate_SortTooLong_ReturnsErrorOnSort()
        {
            var request = new ListingFilterRequestDto { Sort = new string('a', 21) };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.Sort), error.MemberNames);
        }

        // ---------- Required with defaults ----------

        // A missing query parameter keeps the initializer; an empty one binds null.
        [Fact]
        public void New_NoValuesSet_UsesDefaultStatusAndSort()
        {
            var request = new ListingFilterRequestDto();

            Assert.Equal(ListingValues.Available, request.Status);
            Assert.Equal(ListingValues.SortNewest, request.Sort);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_MissingStatus_ReturnsErrorOnStatus(string? status)
        {
            var request = new ListingFilterRequestDto { Status = status! };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.Status), error.MemberNames);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_MissingSort_ReturnsErrorOnSort(string? sort)
        {
            var request = new ListingFilterRequestDto { Sort = sort! };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingFilterRequestDto.Sort), error.MemberNames);
        }
    }
}
