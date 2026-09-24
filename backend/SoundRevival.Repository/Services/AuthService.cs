using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SoundRevival.Dto.Auth;
using SoundRevival.Repository.Entities;
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
            var existingUser = await _repository.FindUserByEmail(request.Email);

            if(existingUser != null)
            {
                throw new InvalidOperationException("Email already registered");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.DisplayName,
                Role = "user",
                CreatedAt = DateTime.UtcNow

            };

            _repository.AddUser(user);
            await _repository.SaveChangesAsync();

            return GenerateAuthResponse(user);
          
        }
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _repository.FindUserByEmail(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");

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
    }
}
