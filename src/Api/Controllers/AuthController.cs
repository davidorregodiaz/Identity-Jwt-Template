using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        var result = await _authService.Login(userLoginDto);
        if (result.IsSuccessful(out var token))
        {
            Response.Cookies.Append("refresh_token", token.RefreshToken);
            return Results.Ok(new
            {
                token = token.AccessToken,
                expires = token.ExpiresIn
            });
        }
            

        return Results.BadRequest(new { error = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterUserDto registerUserDto)
    {
        var result = await _authService.Register(registerUserDto);
        if (result.IsSuccessful(out var token))
        {
            Response.Cookies.Append("refresh_token", token.RefreshToken);
            return Results.Ok(new
            {
                token = token.AccessToken,
                expires = token.ExpiresIn
            });
        }
        return Results.BadRequest(new { error = result.Message });
    }

    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<IResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refresh_token"];

        if (string.IsNullOrEmpty(refreshToken))
            return Results.BadRequest("Refresh token missing");

        var result = await _authService.RefreshToken(refreshToken);

        if (!result.IsSuccessful(out var tokenDto))
            return Results.Ok("Token still valid");

        Response.Cookies.Delete("refresh_token");
        Response.Cookies.Append("refresh_token", tokenDto.RefreshToken);
        
        return Results.Ok(new
        {
            token = tokenDto.AccessToken,
            expires = tokenDto.ExpiresIn
        });
    }

    [HttpPost("logout")]
    public IResult Logout()
    {
        Response.Cookies.Delete("refresh_token");
        return Results.Ok("Logged out");
    }
    
    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Secure=true en producción, false en desarrollo
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };

        Response.Cookies.Append("refresh_token", token, cookieOptions);
    }
}