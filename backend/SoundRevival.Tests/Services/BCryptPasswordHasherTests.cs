using SoundRevival.Infrastructure.Services;

namespace SoundRevival.Tests.Services
{
    public class BCryptPasswordHasherTests
    {
        [Fact]
        public void VerifyPassword_SamePasswordAsHashed_ReturnsTrue()
        {
            var hasher = new BCryptPasswordHasher();
            var hash = hasher.HashPassword("secret");

            var result = hasher.VerifyPassword("secret", hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_DifferentPassword_ReturnsFalse()
        {
            var hasher = new BCryptPasswordHasher();
            var hash = hasher.HashPassword("secret");

            var result = hasher.VerifyPassword("wrong secret", hash);

            Assert.False(result);
        }

        [Fact]
        public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
        {
            var hasher = new BCryptPasswordHasher();
            var hash1 = hasher.HashPassword("secret");
            var hash2 = hasher.HashPassword("secret");

            Assert.NotEqual(hash1, hash2);
            // Different hashes (random salt), yet both verify: the salt is stored inside the hash.
            Assert.True(hasher.VerifyPassword("secret", hash1));
            Assert.True(hasher.VerifyPassword("secret", hash2));
        }
    }
}
