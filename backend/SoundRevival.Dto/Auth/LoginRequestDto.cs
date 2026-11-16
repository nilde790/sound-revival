using System.ComponentModel.DataAnnotations;

namespace SoundRevival.Dto.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        // Only presence and an upper bound: the 8-char minimum is a creation rule,
        // enforcing it here would lock out users whose password predates a stricter policy.
        [Required]
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}
