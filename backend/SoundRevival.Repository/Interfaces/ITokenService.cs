using SoundRevival.Repository.Entities;

namespace SoundRevival.Repository.Interfaces
{
    /// <summary>
    /// Issues access tokens for authenticated users. Keeps token format, signing and expiry
    /// out of the authentication rules (ARCH-5).
    /// </summary>
    public interface ITokenService
    {
        /// <summary>Returns a signed access token carrying the user's id, email and role.</summary>
        string GenerateToken(User user);
    }
}
