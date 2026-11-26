using SoundRevival.Core.Entities;
using SoundRevival.Core.Exceptions;
using SoundRevival.Core.Interfaces;
using SoundRevival.Dto.Auth;
using SoundRevival.Dto.Constants;

namespace SoundRevival.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly ITokenService _tokenService;

    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository repo, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _repository = repo;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var email = NormalizeEmail(request.Email);

        var existingUser = await _repository.FindUserByEmail(email);

        if (existingUser != null)
        {
            throw new BusinessRuleException("Email already registered");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Role = Roles.User,
            CreatedAt = DateTime.UtcNow

        };

        _repository.AddUser(user);
        bool saved;

        try
        {
            saved = await _repository.SaveChangesAsync();
        }
        catch (UniqueConstraintViolationException)
        {
            // Two concurrent registrations passed the check above: the DB unique index is the real guard.
            throw new BusinessRuleException("Email already registered");
        }

        if (!saved)
        {
            throw new InvalidOperationException("Failed to save the new user.");
        }

        return CreateAuthResponse(user);

    }
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var email = NormalizeEmail(request.Email);

        var user = await _repository.FindUserByEmail(email);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Invalid email or password.");

        }

        return CreateAuthResponse(user);
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        return new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Role = user.Role
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
