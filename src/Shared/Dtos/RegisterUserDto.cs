namespace Shared.Dtos;

//Add the fields you need
public record RegisterUserDto(
    string Email,
    string Username,
    string Password);