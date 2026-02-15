using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Products.UpdateProduct
{
    // Todos los campos son opcionales en un update — el cliente solo manda
    // lo que quiere cambiar. Esto se llama "partial update" y es más
    // flexible que requerir el objeto completo en cada modificación.
    public record UpdateProductRequest(
        [MaxLength(128)] string? Name,
        [MaxLength(2048)] string? Description,
        [Range(0.01, double.MaxValue)] decimal? Price,
        [Range(0, int.MaxValue)] int? Stock,
        [MaxLength(512)] string? ImageUrl,
        Guid? CategoryId,
        bool? IsActive
    );
}
