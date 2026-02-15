using MercadoPago.Resource.Order;

namespace ECommerce.API.Common.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // El precio en pesos argentinos. Usamos decimal porque es el tipo
        // correcto para dinero — nunca uses float o double para plata porque
        // tienen errores de punto flotante que pueden causar diferencias de centavos.
        public decimal Price { get; set; }

        public int Stock { get; set; } = 0;

        // URL de la imagen del producto. Por simplicidad guardamos URLs
        // en lugar de manejar uploads — podés usar Cloudinary o S3 para eso.
        public string? ImageUrl { get; set; }

        // Permite ocultar productos sin borrarlos
        public bool IsActive { get; set; } = true;

        public Guid CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación EF Core
        public Category Category { get; set; } = null!;
        public ICollection<CartItem> CartItems { get; set; } = [];
        public ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}
