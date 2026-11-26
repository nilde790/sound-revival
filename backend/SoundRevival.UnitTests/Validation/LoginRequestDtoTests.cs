using SoundRevival.Dto.Auth;

namespace SoundRevival.UnitTests.Validation;

public class LoginRequestDtoTests
{
    private static LoginRequestDto CreateValidRequest() => new()
    {
              Email = "mario@example.com",
        Password = "Password1"
    };

    public static TheoryData<string?> MissingStrings => [(string?)null, "", "   "];

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        var request = CreateValidRequest();

        var results = ValidationHelper.Validate(request);

        Assert.Empty(results);
    }

    // Login checks only presence and upper bounds (see the comment in LoginRequestDto):
    // a short password or an odd email must reach the service, which answers 401.
    [Fact]
    public void Validate_ShortPassword_ReturnsNoErrors()
    {
        var request = CreateValidRequest();
        request.Password = "a";

        var results = ValidationHelper.Validate(request);

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_EmailWithoutFormatCheck_ReturnsNoErrors()
    {
        var request = CreateValidRequest();
        request.Email = "not-an-email";

        var results = ValidationHelper.Validate(request);

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_EmailTooLong_ReturnsErrorOnEmail()
    {
        var request = CreateValidRequest();
        request.Email = new string('a', 256);

        var results = ValidationHelper.Validate(request);

        var error = Assert.Single(results);
        Assert.Contains(nameof(LoginRequestDto.Email), error.MemberNames);
    }

    [Fact]
    public void Validate_PasswordTooLong_ReturnsErrorOnPassword()
    {
        var request = CreateValidRequest();
        request.Password = new string('a', 129);

        var results = ValidationHelper.Validate(request);

        var error = Assert.Single(results);
        Assert.Contains(nameof(LoginRequestDto.Password), error.MemberNames);
    }

    [Theory]
    [MemberData(nameof(MissingStrings))]
    public void Validate_EmailMissing_ReturnsErrorOnEmail(string? email)
    {
        var request = CreateValidRequest();
        request.Email = email!;

        var results = ValidationHelper.Validate(request);

        var error = Assert.Single(results);
        Assert.Contains(nameof(LoginRequestDto.Email), error.MemberNames);
    }

    [Theory]
    [MemberData(nameof(MissingStrings))]
    public void Validate_PasswordMissing_ReturnsErrorOnPassword(string? password)
    {
        var request = CreateValidRequest();
        request.Password = password!;

        var results = ValidationHelper.Validate(request);

        var error = Assert.Single(results);
        Assert.Contains(nameof(LoginRequestDto.Password), error.MemberNames);
    }
}
