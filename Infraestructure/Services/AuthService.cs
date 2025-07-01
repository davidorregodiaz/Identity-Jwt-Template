using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Dtos;
using Shared.Utilities;

namespace Infraestructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;
    
     
    public AuthService(UserManager<AppUser> userManager, IConfiguration configuration, TokenService tokenService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task<TaskResult<TokenModel>> Login(UserLoginDto userLoginDto)
    {
        var user = await _userManager.FindByEmailAsync(userLoginDto.Email);
        if (user is not null)
        {
            var passwordExists = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);
            if (passwordExists)
            {
                var tokenDto = await _tokenService.GenerateTokenDto(user);
                return TaskResult<TokenModel>.FromData(tokenDto);
            }
        }
        return TaskResult<TokenModel>.FromFailure("Invalid email or password");
    }

    public async Task<TaskResult<TokenModel>> Register(RegisterUserDto registerUserDto)
    {
        var appUser = new AppUser(registerUserDto.Email,registerUserDto.Username);
        var result = await _userManager.CreateAsync(appUser, registerUserDto.Password);

        if (result.Succeeded)
        {
            var tokenDto  = await _tokenService.GenerateTokenDto(appUser);
            return TaskResult<TokenModel>.FromData(tokenDto);
        }
        return TaskResult<TokenModel>.FromFailure($"User {registerUserDto.Email} registration failed with errors: {GetErrors(result.Errors)}");
    }
    
    public async Task<TaskResult<TokenModel>> RefreshToken(string refreshToken)
    {
        var user = await _tokenService.FindUserByRefreshToken(refreshToken);
        if (user is null)
            return TaskResult<TokenModel>.FromFailure("Invalid token");
        
        var result = await _tokenService.ValidateRefreshToken(refreshToken, user);
        if (!result.IsSuccessful(out _))
        {
            // Generar nuevo access token
            var newAccessToken = _tokenService.GenerateAccessToken(user);
        
            // Rotar el refresh token (nuevo token)
            var tokenJson = await _tokenService.GenerateRefreshToken(user);
            var newRefreshToken = JsonSerializer.Deserialize<TokenData>(tokenJson);

            return TaskResult<TokenModel>.FromData(new TokenModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken!.Token,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpireMinutes"])
            });
        }
        return TaskResult<TokenModel>.FromFailure("Token is still valid");
    }

    private static string GetErrors(IEnumerable<IdentityError> errors)
    {
        var stringBuilder = new StringBuilder();
        foreach (var error in errors)
        {
            stringBuilder.AppendLine(error.Description);
        }
        return stringBuilder.ToString();
    }
    
}