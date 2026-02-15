namespace ECommerce.API.Common.Models
{
    public enum PaymentStatus
    {
        Pending,    // preferencia creada, el usuario todavía no pagó
        Approved,   // MercadoPago confirmó el pago
        Rejected,   // el pago fue rechazado
        Refunded    // se hizo una devolución
    }

    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }

        // ID de la Preference de MercadoPago — se crea cuando el usuario hace checkout
        public string PreferenceId { get; set; } = string.Empty;

        // ID del pago en MercadoPago — llega via webhook cuando el usuario paga
        // Es null hasta que MercadoPago confirma el pago
        public string? MercadoPagoPaymentId { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación EF Core
        public Order Order { get; set; } = null!;
    }
}
