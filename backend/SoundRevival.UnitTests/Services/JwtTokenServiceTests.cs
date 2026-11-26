using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SoundRevival.Core.Entities;
using SoundRevival.Dto.Constants;
using SoundRevival.Infrastructure.Services;
using SoundRevival.Infrastructure.Settings;

namespace SoundRevival.UnitTests.Services;

public class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Key = "test-signing-key-at-least-32-characters-long",
        Issuer = "SoundRevival",
        Audience = "SoundRevivalUsers",
        ExpiryMinutes = 60
    };

    private readonly JwtTokenService _tokenService = new(Options.Create(Settings));

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "seller@example.com",
        Role = Roles.Admin
    };

    [Fact]
    public void GenerateToken_ValidUser_ContainsIdEmailAndRoleClaims()
    {
        var user = CreateUser();

        var token = new JwtSecurityTokenHandler().ReadJwtToken(_tokenService.GenerateToken(user));

        Assert.Equal(user.Id.ToString(), token.Subject);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        // new JwtSecurityToken(...) writes claim types as given: the role travels under the long ClaimTypes.Role URI.
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == Roles.Admin);
    }

    [Fact]
    public void GenerateToken_ValidUser_ExpiresAfterConfiguredMinutes()
    {
        var before = DateTime.UtcNow;

        var token = new JwtSecurityTokenHandler().ReadJwtToken(_tokenService.GenerateToken(CreateUser()));

        // exp has one-second precision: allow a small window around "now + ExpiryMinutes".
        var expected = before.AddMinutes(Settings.ExpiryMinutes);
        Assert.InRange(token.ValidTo, expected.AddSeconds(-5), expected.AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_ValidUser_PassesValidationWithSameSettings()
    {
        // The same checks JwtBearer runs on every request: signature, issuer, audience, lifetime.
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Key))
        };

        var principal = new JwtSecurityTokenHandler().ValidateToken(
            _tokenService.GenerateToken(CreateUser()), parameters, out _);

        Assert.True(principal.Identity?.IsAuthenticated);
    }

    [Fact]
    public void GenerateToken_DifferentSigningKey_FailsValidation()
    {
        var parameters = new TokenValidationParameters
        {
            ValidIssuer = Settings.Issuer,
            ValidAudience = Settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("another-signing-key-at-least-32-chars-long"))
        };

        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(_tokenService.GenerateToken(CreateUser()), parameters, out _));
    }
}
