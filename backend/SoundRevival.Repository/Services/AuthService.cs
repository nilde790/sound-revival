using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SoundRevival.Dto.Auth;
using SoundRevival.Dto.Constants;
using SoundRevival.Repository.Entities;
using SoundRevival.Repository.Exceptions;
using SoundRevival.Repository.Interfaces;

namespace SoundRevival.Repository.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository repo, IConfiguration configuration)
        {
            _repository = repo;
            _configuration = configuration;
            
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
                DisplayName = request.DisplayName,
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
            var jwtKey = _configuration["Jwt:Key"]!;
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"]!);

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
