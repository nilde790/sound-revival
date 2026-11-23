using SoundRevival.Dto.Constants;
using SoundRevival.Dto.Listings;

namespace SoundRevival.Tests.Validation
{
    public class UpdateListingStatusRequestDtoTests
    {
        public static TheoryData<string?> MissingStrings => [null, "", "   "];

        [Theory]
        [InlineData(ListingValues.Available)]
        [InlineData(ListingValues.Sold)]
        public void Validate_KnownStatus_ReturnsNoErrors(string status)
        {
            var request = new UpdateListingStatusRequestDto { Status = status };

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        // The DTO checks only presence and length: whether the value is an allowed
        // status is a business rule, checked by ListingService.ChangeStateAsync (ADR-010).
        [Fact]
        public void Validate_UnknownStatus_PassesDtoValidation()
        {
            var request = new UpdateListingStatusRequestDto { Status = "reserved" };

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_StatusMissing_ReturnsErrorOnStatus(string? status)
        {
            var request = new UpdateListingStatusRequestDto { Status = status! };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingStatusRequestDto.Status), error.MemberNames);
        }

        [Fact]
        public void Validate_StatusTooLong_ReturnsErrorOnStatus()
        {
            var request = new UpdateListingStatusRequestDto { Status = new string('a', 51) };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(UpdateListingStatusRequestDto.Status), error.MemberNames);
        }
    }
}
