using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Auth.VerifyEmail
{
    public record VerifyEmailRequest(
    [Required][EmailAddress] string Email,
    [Required][Length(6, 6)] string Code
    );
}
