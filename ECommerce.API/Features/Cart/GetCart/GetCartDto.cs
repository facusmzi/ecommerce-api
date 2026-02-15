namespace ECommerce.API.Features.Cart.GetCart
{
    public record CartItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductImageUrl,
    decimal UnitPrice,     // precio actual del producto
    int Quantity,
    decimal Subtotal       // UnitPrice * Quantity, calculado para conveniencia del frontend
    );

    public record CartResponse(
        Guid Id,
        IEnumerable<CartItemResponse> Items,
        decimal Total,         // suma de todos los subtotales
        int ItemCount          // cantidad total de unidades en el carrito
    );
}
