using MercadoPago.Resource.Order;

namespace ECommerce.API.Common.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        // Precio unitario al momento de la compra — inmutable por diseño
        public decimal UnitPrice { get; set; }

        // Navegación EF Core
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
