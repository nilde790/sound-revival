using Microsoft.Extensions.Configuration;
using Moq;
using SoundRevival.Dto.Auth;
using SoundRevival.Repository.Entities;
using SoundRevival.Repository.Interfaces;
using SoundRevival.Repository.Services;
using System;
using System.Collections.Generic;
using System.Text;


namespace SoundRevival.Tests.Mock
{
    public class AuthServiceTests
    {
 
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnAuthResponse()
        {

            var mockUserRepo = new Mock<IUserRepository>();
            var mockConfiguration = new Mock<IConfiguration>();



            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("prova")
            };

            mockUserRepo 
                .Setup(u => u.FindUserByEmail("test@gmail.com"))
                .ReturnsAsync(user);

            mockConfiguration
                .Setup(c => c["Jwt:Key"]).Returns("questa-e-una-chiave-segreta-diversamente-lunga-cambiala-in-produzione-12345");
            mockConfiguration
                .Setup(c => c["Jwt:Issuer"]).Returns("SoundRevival");
            mockConfiguration
                .Setup(c => c["Jwt:Audience"]).Returns("SoundRevivalUsers");
            mockConfiguration
                .Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");




            var authService = new AuthService(mockUserRepo.Object, mockConfiguration.Object);

            var loginRequest = new LoginRequestDto
            {
                Email = "test@gmail.com",
                Password = "prova"
            };

            var result = await authService.LoginAsync(loginRequest);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.False(string.IsNullOrEmpty(result.Token));
      

           
        }
        
    }
}

