namespace ECommerce.API.Features.Categories.GetCategories
{
    // Este DTO es lo que el cliente recibe — no exponemos la entidad
    // de EF Core directamente porque eso acoplaría nuestra API a
    // la estructura interna de la base de datos.
    public record CategoryResponse(
        Guid Id,
        string Name,
        string? Description,
        DateTime CreatedAt
    );
}
