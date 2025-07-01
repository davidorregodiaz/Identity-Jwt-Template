using Shared;
using Shared.Dtos;

namespace Application.Interfaces.Services;

public interface IAuthService
{
    public Task<TaskResult<TokenModel>> Login(UserLoginDto userLoginDto);
    public Task<TaskResult<TokenModel>> Register(RegisterUserDto registerUserDto);
    public Task<TaskResult<TokenModel>> RefreshToken(string refreshToken);
}