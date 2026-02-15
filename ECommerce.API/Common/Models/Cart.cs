namespace ECommerce.API.Common.Models
{
    public class Cart
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación EF Core
        public User User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = [];
    }
}
