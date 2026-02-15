namespace ECommerce.API.Common.Models
{
    public class CartItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }

        // Cuántas unidades de este producto hay en el carrito
        public int Quantity { get; set; } = 1;

        // Navegación EF Core
        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
