using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Auth.Register
{
    public record RegisterRequest(
    [Required][EmailAddress][MaxLength(256)] string Email,
    [Required][MinLength(8)][MaxLength(128)] string Password
    );

    public record RegisterResponse(
        Guid Id,
        string Email,
        bool IsEmailVerified,
        DateTime CreatedAt
    );
}
