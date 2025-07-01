using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Core.Models;
using Infraestructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Dtos;
using Shared.Utilities;

namespace Infraestructure.Services;

public class TokenService
{
    private const string RefreshTokenName = "RefreshToken";
    private const string RefreshTokenProvider = "RefreshTokenProvider";
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public TokenService(IConfiguration configuration, UserManager<AppUser> userManager, AppDbContext context)
    {
        _configuration = configuration;
        this._userManager = userManager;
        _context = context;
    }

    public string GenerateAccessToken(AppUser user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!));
        
        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<string> GenerateRefreshToken(AppUser user)
    {

        var refreshToken = new TokenData()
        {
            Token = Guid.NewGuid().ToString("N"),
            Expiration = DateTime.UtcNow.AddDays(7)
        };

        var json = JsonSerializer.Serialize(refreshToken);
        
        await _userManager.RemoveAuthenticationTokenAsync(
            user, 
            RefreshTokenProvider, 
            RefreshTokenName);
        
        await _userManager.SetAuthenticationTokenAsync(
            user, 
            RefreshTokenProvider, 
            RefreshTokenName, 
            json);
        
        return json;
    }

    public async Task<TokenModel> GenerateTokenDto(AppUser appUser)
    {
        var accessToken = GenerateAccessToken(appUser);
        var refreshToken = await GenerateRefreshToken(appUser);
        return new TokenModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = Convert.ToInt32(_configuration["Jwt:ExpireMinutes"])
        };
    }
    
    
    public async Task<AppUser?> FindUserByRefreshToken(string refreshToken)
    {
        // Inyectar AppDbContext en el constructor para usar este método
        var tokenEntry = await _context.UserTokens
            .FirstOrDefaultAsync(t => 
                t.LoginProvider == RefreshTokenProvider &&
                t.Name == RefreshTokenName &&
                t.Value == refreshToken);
        
        if (tokenEntry == null)
            return null;
        
        return await _userManager.FindByIdAsync(tokenEntry.UserId);
    }

    public async Task<TaskResult<TokenData>> ValidateRefreshToken(string refreshToken,AppUser appUser)
    {
        //Obtenemos el token del usuario
        var currentToken = await _userManager.GetAuthenticationTokenAsync(appUser, RefreshTokenProvider, RefreshTokenName);
        var tokenData = JsonSerializer.Deserialize<TokenData>(currentToken);
        
        if (tokenData is null || tokenData.Token != refreshToken)
            return TaskResult<TokenData>.FromFailure("Token mismatch");
        
        if (tokenData.Expiration < DateTime.UtcNow)
            return TaskResult<TokenData>.FromFailure("Token expired");

        return TaskResult<TokenData>.FromData(tokenData);
    }

}