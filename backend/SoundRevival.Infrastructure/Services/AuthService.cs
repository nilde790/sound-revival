using Microsoft.EntityFrameworkCore;
using Npgsql;
using SoundRevival.Dto.Auth;
using SoundRevival.Dto.Constants;
using SoundRevival.Infrastructure.Entities;
using SoundRevival.Infrastructure.Exceptions;
using SoundRevival.Infrastructure.Interfaces;

namespace SoundRevival.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository repo, ITokenService tokenService)
        {
            _repository = repo;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var email = NormalizeEmail(request.Email);

            var existingUser = await _repository.FindUserByEmail(email);

            if(existingUser != null)
            {
                throw new BusinessRuleException("Email already registered");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
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
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
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

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
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
}
