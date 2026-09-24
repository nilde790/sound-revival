using System;
using System.Collections.Generic;
using System.Text;
using SoundRevival.Dto.Auth;

namespace SoundRevival.Repository.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    }
}
