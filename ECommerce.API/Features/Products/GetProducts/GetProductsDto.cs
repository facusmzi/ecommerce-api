namespace ECommerce.API.Features.Products.GetProducts
{
    // Este DTO captura todos los parámetros de filtrado y paginación
    // que el cliente puede enviar como query params en la URL.
    // Por ejemplo: GET /products?categoryId=...&search=laptop&page=2&pageSize=10
    public record GetProductsRequest(
        // Filtro opcional por categoría
        Guid? CategoryId,

        // Búsqueda de texto libre — va a buscar en nombre y descripción
        string? Search,

        // Paginación — con valores por defecto sensatos
        int Page = 1,
        int PageSize = 20
    );

    public record ProductResponse(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        int Stock,
        string? ImageUrl,
        bool IsActive,
        Guid CategoryId,
        string CategoryName,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    // Este wrapper es el patrón estándar para respuestas paginadas.
    // Le da al cliente toda la información que necesita para construir
    // controles de paginación en el frontend.
    public record PagedResponse<T>(
        IEnumerable<T> Items,  // los resultados de esta página
        int TotalItems,        // total de resultados sin paginar
        int Page,              // página actual
        int PageSize,          // tamaño de página
        int TotalPages         // cuántas páginas hay en total
    );
}
