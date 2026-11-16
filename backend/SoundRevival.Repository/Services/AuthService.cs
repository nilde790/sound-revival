using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SoundRevival.Dto.Auth;
using SoundRevival.Dto.Constants;
using SoundRevival.Repository.Entities;
using SoundRevival.Repository.Exceptions;
using SoundRevival.Repository.Interfaces;
using SoundRevival.Repository.Settings;

namespace SoundRevival.Repository.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUserRepository repo, IOptions<JwtSettings> jwtoptions)
        {
            _repository = repo;
            _jwtSettings = jwtoptions.Value;
            
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

            return GenerateAuthResponse(user);
          
        }
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var email = NormalizeEmail(request.Email);

            var user = await _repository.FindUserByEmail(email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException("Invalid email or password.");

            }

            return GenerateAuthResponse(user);
        }

        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var jwtKey = _jwtSettings.Key;
            var jwtIssuer = _jwtSettings.Issuer;
            var jwtAudience = _jwtSettings.Audience;
            var expiryMinutes = _jwtSettings.ExpiryMinutes;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
                
                );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
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
