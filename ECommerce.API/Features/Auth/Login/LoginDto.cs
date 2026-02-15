using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Auth.Login
{
    public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

    public record LoginResponse(
        string Token,
        DateTime ExpiresAt,
        UserDto User
    );

    public record UserDto(
        Guid Id,
        string Email,
        bool IsEmailVerified,
        bool IsAdmin
    );
}
