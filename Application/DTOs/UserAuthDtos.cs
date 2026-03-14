namespace Application.DTOs
{
    public record CreateUserDto(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string? PhoneNumber
    );

    public record UpdateUserDto(
        Guid Id,
        string FirstName,
        string LastName,
        string? PhoneNumber
    );

    public record LoginDto(
        string Email,
        string Password
    );

    public record AuthResponseDto(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        UserDto User
    );
}
