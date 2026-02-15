namespace ECommerce.API.Features.Orders.GetOrderById
{
    public record OrderItemDetailResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
    );

    public record OrderDetailResponse(
        Guid Id,
        string Status,
        decimal Total,
        IEnumerable<OrderItemDetailResponse> Items,
        string? PaymentStatus,   // null si todavía no se inició el pago
        string? InitPoint,       // la URL de MercadoPago — null si no hay preferencia creada
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
