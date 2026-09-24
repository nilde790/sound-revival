using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Dto.Auth
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password {  get; set; } = string.Empty;

        public string DisplayName {  get; set; } = string.Empty;
    }
}
