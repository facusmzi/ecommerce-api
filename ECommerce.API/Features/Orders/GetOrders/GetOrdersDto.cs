namespace ECommerce.API.Features.Orders.GetOrders
{
    public record OrderSummaryResponse(
    Guid Id,
    string Status,
    decimal Total,
    int ItemCount,
    DateTime CreatedAt
    );
}
