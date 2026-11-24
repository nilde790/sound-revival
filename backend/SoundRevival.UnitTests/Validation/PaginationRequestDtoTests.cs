using SoundRevival.Dto.Common;

namespace SoundRevival.UnitTests.Validation
{
    public class PaginationRequestDtoTests
    {
        [Fact]
        public void Validate_DefaultValues_ReturnsNoErrors()
        {
            var request = new PaginationRequestDto();

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10000)]
        public void Validate_PageAtBoundary_ReturnsNoErrors(int page)
        {
            var request = new PaginationRequestDto { Page = page };

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(10001)]
        [InlineData(int.MaxValue)]
        public void Validate_PageOutOfRange_ReturnsErrorOnPage(int page)
        {
            var request = new PaginationRequestDto { Page = page };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(PaginationRequestDto.Page), error.MemberNames);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Validate_PageSizeAtBoundary_ReturnsNoErrors(int pageSize)
        {
            var request = new PaginationRequestDto { PageSize = pageSize };

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_PageSizeOutOfRange_ReturnsErrorOnPageSize(int pageSize)
        {
            var request = new PaginationRequestDto { PageSize = pageSize };

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(PaginationRequestDto.PageSize), error.MemberNames);
        }
    }
}
