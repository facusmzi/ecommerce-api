using MercadoPago.Resource.Payment;

namespace ECommerce.API.Common.Models
{

    public enum OrderStatus
    {
        Pending,    // orden creada, esperando pago
        Paid,       // MercadoPago confirmó el pago via webhook
        Shipped,    // el vendedor marcó como enviado
        Delivered,  // entregado al cliente
        Cancelled   // cancelado por cualquier motivo
    }

    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Total calculado al momento de crear la orden.
        // También lo guardamos por la misma razón que UnitPrice en OrderItem.
        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación EF Core
        public User User { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = [];
        public Payment? Payment { get; set; }
    }
}
