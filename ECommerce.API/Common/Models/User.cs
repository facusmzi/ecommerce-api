using MercadoPago.Resource.Order;
using static System.Collections.Specialized.BitVector32;

namespace ECommerce.API.Common.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationCode { get; set; }

        // En lugar de IsCreator usamos IsAdmin — más claro en el contexto de un e-commerce
        public bool IsAdmin { get; set; } = false;

        public Guid? ProfileId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación EF Core
        public ICollection<Session> Sessions { get; set; } = [];
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = [];
    }
}
