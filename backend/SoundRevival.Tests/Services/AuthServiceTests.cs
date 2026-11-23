using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using SoundRevival.Dto.Auth;
using SoundRevival.Dto.Constants;
using SoundRevival.Infrastructure.Entities;
using SoundRevival.Infrastructure.Exceptions;
using SoundRevival.Infrastructure.Interfaces;
using SoundRevival.Infrastructure.Services;

namespace SoundRevival.Tests.Services
{
    public class AuthServiceTests
    {
        // xUnit creates a new instance of this class for every test,
        // so these fields are fresh (and the mock has no leftover setups) each time.
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly AuthService _authService;

        private const string FakeToken = "fake-token";

        public AuthServiceTests()
        {
            // Token creation is JwtTokenService's job (tested separately): here it only has to return something.
            _tokenService.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns(FakeToken);

            _authService = new AuthService(_userRepo.Object, _tokenService.Object);
        }

        private static User CreateUser(string email, string password) => new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        // ---------- LoginAsync ----------

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsTokenAndUserId()
        {
            var user = CreateUser("test@gmail.com", "prova");
            _userRepo.Setup(r => r.FindUserByEmail("test@gmail.com")).ReturnsAsync(user);

            var result = await _authService.LoginAsync(new LoginRequestDto { Email = "test@gmail.com", Password = "prova" });

            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(FakeToken, result.Token);
            _tokenService.Verify(t => t.GenerateToken(user), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_DoesNotGenerateToken()
        {
            _userRepo.Setup(r => r.FindUserByEmail("test@gmail.com")).ReturnsAsync(CreateUser("test@gmail.com", "correct"));

            await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
                _authService.LoginAsync(new LoginRequestDto { Email = "test@gmail.com", Password = "wrong" }));

            _tokenService.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentials()
        {
            _userRepo.Setup(r => r.FindUserByEmail("unknown@gmail.com")).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
                _authService.LoginAsync(new LoginRequestDto { Email = "unknown@gmail.com", Password = "any" }));
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
        {
            _userRepo.Setup(r => r.FindUserByEmail("test@gmail.com")).ReturnsAsync(CreateUser("test@gmail.com", "correct"));

            await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
                _authService.LoginAsync(new LoginRequestDto { Email = "test@gmail.com", Password = "wrong" }));
        }

        [Fact]
        public async Task LoginAsync_EmailWithDifferentCaseAndSpaces_FindsUserByNormalizedEmail()
        {
            var user = CreateUser("test@gmail.com", "prova");
            _userRepo.Setup(r => r.FindUserByEmail("test@gmail.com")).ReturnsAsync(user);

            var result = await _authService.LoginAsync(new LoginRequestDto { Email = " Test@Gmail.COM ", Password = "prova" });

            Assert.Equal(user.Id, result.UserId);
        }

        // ---------- RegisterAsync ----------

        [Fact]
        public async Task RegisterAsync_EmailAlreadyRegistered_ThrowsBusinessRule()
        {
            _userRepo.Setup(r => r.FindUserByEmail("existing@gmail.com")).ReturnsAsync(CreateUser("existing@gmail.com", "test"));

            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _authService.RegisterAsync(new RegisterRequestDto { Email = "existing@gmail.com", Password = "test" }));
        }

        [Fact]
        public async Task RegisterAsync_ValidRequest_StoresNormalizedEmailUserRoleAndHashedPassword()
        {
            User? savedUser = null;
            _userRepo.Setup(r => r.FindUserByEmail("new@gmail.com")).ReturnsAsync((User?)null);
            _userRepo.Setup(r => r.AddUser(It.IsAny<User>())).Callback<User>(u => savedUser = u);
            _userRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

            var result = await _authService.RegisterAsync(new RegisterRequestDto { Email = "  New@Gmail.COM ", Password = "secret" });

            Assert.NotNull(savedUser);
            Assert.Equal("new@gmail.com", savedUser!.Email);
            Assert.Equal(Roles.User, savedUser.Role);
            Assert.NotEqual("secret", savedUser.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("secret", savedUser.PasswordHash));
            Assert.Equal(savedUser.Id, result.UserId);
            Assert.Equal(FakeToken, result.Token);
            _tokenService.Verify(t => t.GenerateToken(savedUser), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_UniqueViolationOnSave_ThrowsBusinessRule()
        {
            _userRepo.Setup(r => r.FindUserByEmail("race@gmail.com")).ReturnsAsync((User?)null);
            _userRepo.Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateException("save failed",
                    new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation)));

            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _authService.RegisterAsync(new RegisterRequestDto { Email = "race@gmail.com", Password = "x" }));
        }

        [Fact]
        public async Task RegisterAsync_OtherDatabaseError_IsNotTranslated()
        {
            _userRepo.Setup(r => r.FindUserByEmail("fk@gmail.com")).ReturnsAsync((User?)null);
            _userRepo.Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateException("save failed",
                    new PostgresException("fk violation", "ERROR", "ERROR", PostgresErrorCodes.ForeignKeyViolation)));

            await Assert.ThrowsAsync<DbUpdateException>(() =>
                _authService.RegisterAsync(new RegisterRequestDto { Email = "fk@gmail.com", Password = "x" }));
        }

        [Fact]
        public async Task RegisterAsync_SaveReturnsFalse_ThrowsInvalidOperation()
        {
            _userRepo.Setup(r => r.FindUserByEmail("nosave@gmail.com")).ReturnsAsync((User?)null);
            _userRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync(new RegisterRequestDto { Email = "nosave@gmail.com", Password = "x" }));
        }
    }
}
