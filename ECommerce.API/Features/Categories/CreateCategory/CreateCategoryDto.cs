using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Categories.CreateCategory
{

    public record CreateCategoryRequest(
    [Required][MaxLength(64)] string Name,
    [MaxLength(512)] string? Description
    );

    public record CreateCategoryResponse(
        Guid Id,
        string Name,
        string? Description,
        DateTime CreatedAt
    );


}
