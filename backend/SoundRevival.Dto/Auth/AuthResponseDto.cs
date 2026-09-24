using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Dto.Auth
{
    public class AuthResponseDto
    {
        public string Token {  get; set; } = string.Empty;
        public Guid UserId {  get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
