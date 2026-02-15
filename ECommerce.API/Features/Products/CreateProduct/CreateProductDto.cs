using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Products.CreateProduct
{
    public record CreateProductRequest(
    // Los campos requeridos van primero, sin valores por defecto
    [Required][MaxLength(128)] string Name,
    [Required][Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
    decimal Price,
    [Required] Guid CategoryId,

    // Los campos opcionales van al final, con sus valores por defecto
    [MaxLength(2048)] string? Description = null,
    [Range(0, int.MaxValue)] int Stock = 0,
    [MaxLength(512)] string? ImageUrl = null
    );

    public record CreateProductResponse(
        Guid Id,
        string Name,
        decimal Price,
        int Stock,
        Guid CategoryId,
        string CategoryName,
        DateTime CreatedAt
    );
}
