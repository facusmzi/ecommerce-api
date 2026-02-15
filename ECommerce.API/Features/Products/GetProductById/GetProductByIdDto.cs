namespace ECommerce.API.Features.Products.GetProductById
{
    public record ProductDetailResponse(
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
}
