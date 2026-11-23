using System.ComponentModel.DataAnnotations;

namespace SoundRevival.Dto.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
