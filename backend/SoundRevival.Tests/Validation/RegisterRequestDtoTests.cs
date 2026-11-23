using SoundRevival.Dto.Auth;

namespace SoundRevival.Tests.Validation
{
    public class RegisterRequestDtoTests
    {
        private static RegisterRequestDto CreateValidRequest() => new()
        {
            Email = "mario@example.com",
            Password = "Password1",
            DisplayName = "Mario"
        };

        public static TheoryData<string?> MissingStrings => [null, "", "   "];

        [Fact]
        public void Validate_ValidRequest_ReturnsNoErrors()
        {
            var request = CreateValidRequest();

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        // ---------- Email ----------

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("mario@")]
        [InlineData("@example.com")]
        [InlineData("mario@@example.com")]
        public void Validate_MalformedEmail_ReturnsErrorOnEmail(string email)
        {
            var request = CreateValidRequest();
            request.Email = email;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(RegisterRequestDto.Email), error.MemberNames);
        }

        [Fact]
        public void Validate_EmailAtMaxLength_ReturnsNoErrors()
        {
            var request = CreateValidRequest();
            request.Email = new string('a', 243) + "@example.com"; // 255 chars

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_EmailTooLong_ReturnsErrorOnEmail()
        {
            var request = CreateValidRequest();
            request.Email = new string('a', 244) + "@example.com"; // 256 chars, still well-formed

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(RegisterRequestDto.Email), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_EmailMissing_ReturnsErrorOnEmail(string? email)
        {
            var request = CreateValidRequest();
            request.Email = email!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(RegisterRequestDto.Email), error.MemberNames);
        }

        // ---------- Password ----------

        [Theory]
        [InlineData(8)]
        [InlineData(128)]
        public void Validate_PasswordLengthAtBoundary_ReturnsNoErrors(int length)
        {
            var request = CreateValidRequest();
            request.Password = new string('a', length);

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Theory]
        [InlineData(7)]
        [InlineData(129)]
        public void Validate_PasswordLengthOutOfRange_ReturnsErrorOnPassword(int length)
        {
            var request = CreateValidRequest();
            request.Password = new string('a', length);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(RegisterRequestDto.Password), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_PasswordMissing_ReturnsErrorOnPassword(string? password)
        {
            var request = CreateValidRequest();
            request.Password = password!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(RegisterRequestDto.Password), error.MemberNames);
        }

        // ---------- DisplayName ----------

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Validate_DisplayNameLengthAtBoundary_ReturnsNoErrors(int length)
        {
            var request = CreateValidRequest();
            request.DisplayName = new string('a', length);

            var results = ValidationHelper.Validate(request);

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_DisplayNameTooLong_ReturnsErrorOnDisplayName()
        {
            var request = CreateValidRequest();
            request.DisplayName = new string('a', 101);

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(RegisterRequestDto.DisplayName), error.MemberNames);
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public void Validate_DisplayNameMissing_ReturnsErrorOnDisplayName(string? displayName)
        {
            var request = CreateValidRequest();
            request.DisplayName = displayName!;

            var results = ValidationHelper.Validate(request);

            var error = Assert.Single(results);
            Assert.Contains(nameof(RegisterRequestDto.DisplayName), error.MemberNames);
        }
    }
}
