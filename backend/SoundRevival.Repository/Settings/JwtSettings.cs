using System.ComponentModel.DataAnnotations;

namespace SoundRevival.Repository.Settings
{
    public class JwtSettings
    {
        [Required]
        public required string Key {  get; set; }
        [Required]
        public required string Issuer { get; set; }
        [Required]
        public required string Audience {  get; set; }
        [Range(1, 1440)]
        public required int ExpiryMinutes { get; set; }

    }
}
