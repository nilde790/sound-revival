using SoundRevival.Dto.Listings;

namespace SoundRevival.Tests.Validation
{
    // Mirrors CreateListingRequestDtoTests: the two DTOs duplicate the same rules
    // on purpose (debt C12), so a rule changed in only one of them breaks a test here.
    public class UpdateListingRequestDtoTests
    {
        private static UpdateListingRequestDto CreateValidRequest() => new()
        {
            Title = "Fender Stratocaster",
            Description = "Electric guitar, minor scratches",
            Price = 800m,
            Category = "Guitars",
            Condition = "Good"
        };

        public static TheoryData<string?> MissingStrings => [null, "", "   "];

        public static TheoryData<decimal> ValidPrices => [0.01m, 1000000m, 450.50m];

        // Each value breaks exactly one rule (0.001m would break both Range and MaxDecimalPlaces).
        public static TheoryData<decimal> OutOfRangePrices => [0m, -1m, 1000000.01m];

        [Fact]
        public void Validate_ValidRequest_ReturnsNoErrors()
        {
            var request = CreateValidRequest();

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_StringsAtMaxLength_ReturnsNoErrors()
        {
            var request = CreateValidRequest();
            request.Title = new string('a', 150);
            request.Description = new string('a', 5000);
            request.Category = new string('a', 50);
            request.Condition = new string('a', 50);

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        // ---------- Too long ----------

        [Fact]
        public void Validate_TitleTooLong_ReturnsErrorOnTitle()
        {
            var request = CreateValidRequest();
            request.Title = new string('a', 151);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Title), error.MemberNames);
        }

        [Fact]
        public void Validate_DescriptionTooLong_ReturnsErrorOnDescription()
        {
            var request = CreateValidRequest();
            request.Description = new string('a', 5001);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Description), error.MemberNames);
        }

        [Fact]
        public void Validate_CategoryTooLong_ReturnsErrorOnCategory()
        {
            var request = CreateValidRequest();
            request.Category = new string('a', 51);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Category), error.MemberNames);
        }

        [Fact]
        public void Validate_ConditionTooLong_ReturnsErrorOnCondition()
        {
            var request = CreateValidRequest();
            request.Condition = new string('a', 51);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Condition), error.MemberNames);
        }

        // ---------- Missing ----------

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_TitleMissing_ReturnsErrorOnTitle(string? title)
        {
            var request = CreateValidRequest();
            request.Title = title!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Title), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_DescriptionMissing_ReturnsErrorOnDescription(string? description)
        {
            var request = CreateValidRequest();
            request.Description = description!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Description), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_CategoryMissing_ReturnsErrorOnCategory(string? category)
        {
            var request = CreateValidRequest();
            request.Category = category!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Category), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_ConditionMissing_ReturnsErrorOnCondition(string? condition)
        {
            var request = CreateValidRequest();
            request.Condition = condition!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Condition), error.MemberNames);
        }

        // ---------- Price ----------

        [Theory]
        [MemberData(nameof(ValidPrices))]
        public void Validate_PriceInRange_ReturnsNoErrors(decimal price)
        {
            var request = CreateValidRequest();
            request.Price = price;

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [MemberData(nameof(OutOfRangePrices))]
        public void Validate_PriceOutOfRange_ReturnsErrorOnPrice(decimal price)
        {
            var request = CreateValidRequest();
            request.Price = price;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Price), error.MemberNames);
        }

        [Fact]
        public void Validate_PriceWithThreeDecimals_ReturnsErrorOnPrice()
        {
            var request = CreateValidRequest();
            request.Price = 1.001m;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingRequestDto.Price), error.MemberNames);
        }
    }
}
