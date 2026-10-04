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

        [Fact]

        public async Task LoginAsync_UserDoesNotExist_ReturnException()
        {
            var mockUserRepo = new Mock<IUserRepository>();
            var mockConfiguration = new Mock<IConfiguration>();


            mockConfiguration
                .Setup(c => c["Jwt:Key"]).Returns("questa-e-una-chiave-segreta-diversamente-lunga-cambiala-in-produzione-12345");
            mockConfiguration
                .Setup(c => c["Jwt:Issuer"]).Returns("SoundRevival");
            mockConfiguration
                .Setup(c => c["Jwt:Audience"]).Returns("SoundRevivalUsers");
            mockConfiguration
                .Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");



            mockUserRepo
              .Setup(u => u.FindUserByEmail("notexistent@gmail.com"))
              .ReturnsAsync((User?)null);

            var authService = new AuthService(mockUserRepo.Object, mockConfiguration.Object);

            var loginRequest = new LoginRequestDto
            {
                Email = "notexistent@gmail.com",
                Password = "any"
            };


            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(loginRequest));
        }

        [Fact]

        public async Task LoginAsync_InvalidCredentials_ReturnException()
        {
            var mockUserRepo = new Mock<IUserRepository>();
            var mockConfiguration = new Mock<IConfiguration>();


            mockConfiguration
                .Setup(c => c["Jwt:Key"]).Returns("questa-e-una-chiave-segreta-diversamente-lunga-cambiala-in-produzione-12345");
            mockConfiguration
                .Setup(c => c["Jwt:Issuer"]).Returns("SoundRevival");
            mockConfiguration
                .Setup(c => c["Jwt:Audience"]).Returns("SoundRevivalUsers");
            mockConfiguration
                .Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "invalidpassword@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct")


            };

            mockUserRepo
              .Setup(u => u.FindUserByEmail("invalidpassword@gmail.com"))
              .ReturnsAsync(user);

            var authService = new AuthService(mockUserRepo.Object, mockConfiguration.Object);

         

            var loginRequest = new LoginRequestDto
            {
                Email = "invalidpassword@gmail.com",
                Password = "wrong"
            };


            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(loginRequest));
        }

        [Fact]

        public async Task RegisterAsync_UserAlreadyExist_ReturnException()
        {
            var mockUserRepo = new Mock<IUserRepository>();
            var mockConfiguration = new Mock<IConfiguration>();


            mockConfiguration
                .Setup(c => c["Jwt:Key"]).Returns("questa-e-una-chiave-segreta-diversamente-lunga-cambiala-in-produzione-12345");
            mockConfiguration
                .Setup(c => c["Jwt:Issuer"]).Returns("SoundRevival");
            mockConfiguration
                .Setup(c => c["Jwt:Audience"]).Returns("SoundRevivalUsers");
            mockConfiguration
                .Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "alreadyexisting@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("test")


            };

            mockUserRepo
              .Setup(u => u.FindUserByEmail("alreadyexisting@gmail.com"))
              .ReturnsAsync(user);

            var authService = new AuthService(mockUserRepo.Object, mockConfiguration.Object);


            var registerRequest = new RegisterRequestDto()
            {
                Email = "alreadyexisting@gmail.com",
                Password ="test"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => authService.RegisterAsync(registerRequest));
        }


        [Fact]

        public async Task RegisterAsync_UserCreated_Returnresponse()
        {
            var mockUserRepo = new Mock<IUserRepository>();
            var mockConfiguration = new Mock<IConfiguration>();


            mockConfiguration
                .Setup(c => c["Jwt:Key"]).Returns("questa-e-una-chiave-segreta-diversamente-lunga-cambiala-in-produzione-12345");
            mockConfiguration
                .Setup(c => c["Jwt:Issuer"]).Returns("SoundRevival");
            mockConfiguration
                .Setup(c => c["Jwt:Audience"]).Returns("SoundRevivalUsers");
            mockConfiguration
                .Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

    

            mockUserRepo
              .Setup(u => u.FindUserByEmail("newuser@gmail.com"))
              .ReturnsAsync((User?)null);

            mockUserRepo.Setup(r => r.AddUser(It.IsAny<User>()));

            mockUserRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);



            var authService = new AuthService(mockUserRepo.Object, mockConfiguration.Object);


            var registerRequest = new RegisterRequestDto()
            {
                Email = "newuser@gmail.com",
                Password = "new"
         
            };

            

            var result = await authService.RegisterAsync(registerRequest);
            Assert.NotNull(result);
        }



    }
}

