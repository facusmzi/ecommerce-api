namespace ECommerce.API.Features.Orders.CreateOrder
{
    public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,   // precio al momento de la compra — inmutable
    decimal Subtotal
);

    public record CreateOrderResponse(
        Guid Id,
        string Status,
        decimal Total,
        IEnumerable<OrderItemResponse> Items,
        DateTime CreatedAt
    );
}
