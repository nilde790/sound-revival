using SoundRevival.Dto.Listings;

namespace SoundRevival.UnitTests.Validation
{
    public class CreateListingRequestDtoTests
    {

        private static CreateListingRequestDto CreateValidRequest() => new()
        {
            Title = "Yamaha P-125 test",
            Description = "Digital piano, like new",
            Price = 450m,
            Category = "Keyboards",
            Condition = "Like New"
        };
        public static TheoryData<string?> MissingStrings => [null, "", "   "];


        [Fact]
        public void Validate_TitleTooLong_ReturnsErrorOnTitle()
        {
            var request = CreateValidRequest();
            request.Title = new string('a', 151);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Title), error.MemberNames);
        }


        [Fact]
        public void Validate_ValidRequest_ReturnsNoErrors()
        {
            var request = CreateValidRequest();

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        // ---------- Title ----------

        [Theory]
        [InlineData(1)]
        [InlineData(150)]
        public void Validate_TitleLengthAtBoundary_ReturnsNoErrors(int length)
        {
            var request = CreateValidRequest();
            request.Title = new string('a', length);

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_TitleMissing_ReturnsErrorOnTitle(string? title)
        {
            var request = CreateValidRequest();
            request.Title = title!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Title), error.MemberNames);
        }

        // ---------- Description ----------

        [Fact]
        public void Validate_DescriptionAtMaxLength_ReturnsNoErrors()
        {
            var request = CreateValidRequest();
            request.Description = new string('a', 5000);

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_DescriptionTooLong_ReturnsErrorOnDescription()
        {
            var request = CreateValidRequest();
            request.Description = new string('a', 5001);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Description), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_DescriptionMissing_ReturnsErrorOnDescription(string? description)
        {
            var request = CreateValidRequest();
            request.Description = description!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Description), error.MemberNames);
        }

        // ---------- Category ----------

        [Fact]
        public void Validate_CategoryAtMaxLength_ReturnsNoErrors()
        {
            var request = CreateValidRequest();
            request.Category = new string('a', 50);

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_CategoryTooLong_ReturnsErrorOnCategory()
        {
            var request = CreateValidRequest();
            request.Category = new string('a', 51);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Category), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_CategoryMissing_ReturnsErrorOnCategory(string? category)
        {
            var request = CreateValidRequest();
            request.Category = category!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Category), error.MemberNames);
        }

        // ---------- Condition ----------

        [Fact]
        public void Validate_ConditionAtMaxLength_ReturnsNoErrors()
        {
            var request = CreateValidRequest();
            request.Condition = new string('a', 50);

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_ConditionTooLong_ReturnsErrorOnCondition()
        {
            var request = CreateValidRequest();
            request.Condition = new string('a', 51);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Condition), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_ConditionMissing_ReturnsErrorOnCondition(string? condition)
        {
            var request = CreateValidRequest();
            request.Condition = condition!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Condition), error.MemberNames);
        }

        // ---------- Price ----------

        public static TheoryData<decimal> ValidPrices => [0.01m, 1000000m, 450.50m];

        // Each value breaks exactly one rule, so Assert.Single holds.
        // 0.001m is left out on purpose: it breaks both Range and MaxDecimalPlaces.
        public static TheoryData<decimal> OutOfRangePrices => [0m, -1m, 1000000.01m];

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
            Assert.Contains(nameof(CreateListingRequestDto.Price), error.MemberNames);
        }

        [Fact]
        public void Validate_PriceWithThreeDecimals_ReturnsErrorOnPrice()
        {
            var request = CreateValidRequest();
            request.Price = 1.001m;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(CreateListingRequestDto.Price), error.MemberNames);
        }
    }
}
