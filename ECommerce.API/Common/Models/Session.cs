namespace ECommerce.API.Common.Models
{
    public class Session
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Address { get; set; } = string.Empty;
        public string Device { get; set; } = "unknown";
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Location { get; set; } = "Unknown, Unknown";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public string? Method { get; set; }

        // Navegación EF Core
        public User User { get; set; } = null!;
    }
}
