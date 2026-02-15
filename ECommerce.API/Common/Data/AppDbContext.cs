using ECommerce.API.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Common.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // Cada DbSet<T> se convierte en una tabla en SQL Server.
        // EF Core usa el nombre de la propiedad como nombre de la tabla por convención.
        public DbSet<User> Users => Set<User>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                // El email tiene que ser único en toda la tabla.
                // EF Core crea un índice único en SQL Server para garantizarlo.
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
                entity.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();

                // Un usuario tiene muchas sesiones (1:N)
                entity.HasMany(u => u.Sessions)
                      .WithOne(s => s.User)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // si borrás el usuario, se borran sus sesiones

                // Un usuario tiene un solo carrito (1:1)
                // El carrito no puede existir sin el usuario
                entity.HasOne(u => u.Cart)
                      .WithOne(c => c.User)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Un usuario puede tener muchas órdenes (1:N)
                entity.HasMany(u => u.Orders)
                      .WithOne(o => o.User)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // no permitimos borrar usuarios con órdenes
            });

            // ── Session ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.Token);   // para buscar por token eficientemente
                entity.HasIndex(s => s.UserId);  // para listar sesiones por usuario
                entity.Property(s => s.Token).HasMaxLength(1024).IsRequired();
                entity.Property(s => s.Device).HasMaxLength(512);
                entity.Property(s => s.Address).HasMaxLength(64);
                entity.Property(s => s.Location).HasMaxLength(128);
            });

            // ── Category ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(64).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(512);

                // Una categoría tiene muchos productos (1:N)
                // Restrict porque no queremos borrar accidentalmente todos los productos
                // de una categoría si la eliminamos
                entity.HasMany(c => c.Products)
                      .WithOne(p => p.Category)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Product ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(128).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(2048);

                // Para precios siempre especificamos precisión y escala explícitamente.
                // precision: 18 significa hasta 18 dígitos en total.
                // scale: 2 significa 2 dígitos después del punto decimal.
                // Esto se traduce a DECIMAL(18,2) en SQL Server.
                entity.Property(p => p.Price)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(p => p.ImageUrl).HasMaxLength(512);

                // Índice en IsActive para que filtrar productos activos sea rápido.
                // Esta es una consulta que va a ocurrir en absolutamente todos los
                // requests del catálogo, así que el índice vale mucho la pena.
                entity.HasIndex(p => p.IsActive);
            });

            // ── Cart ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.Id);

                // Un carrito tiene muchos items (1:N)
                // Si borramos el carrito, sus items se borran también
                entity.HasMany(c => c.Items)
                      .WithOne(i => i.Cart)
                      .HasForeignKey(i => i.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── CartItem ──────────────────────────────────────────────────────────
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);

                // Un usuario no debería poder tener el mismo producto dos veces
                // en el carrito como ítems separados — debería incrementar la cantidad.
                // Este índice único lo garantiza a nivel de base de datos.
                entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();

                entity.HasOne(ci => ci.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Order ─────────────────────────────────────────────────────────────
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                // Guardamos el enum como string en la DB ("Pending", "Paid", etc.)
                // en lugar de como número (0, 1, 2...). Esto hace que la base de datos
                // sea legible directamente sin necesidad de un diccionario de referencia.
                entity.Property(o => o.Status)
                      .HasConversion<string>()
                      .HasMaxLength(32);

                entity.Property(o => o.Total).HasPrecision(18, 2).IsRequired();

                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Una orden tiene un solo pago (1:1, opcional porque el pago
                // se crea cuando el usuario hace checkout, no al crear la orden)
                entity.HasOne(o => o.Payment)
                      .WithOne(p => p.Order)
                      .HasForeignKey<Payment>(p => p.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── OrderItem ─────────────────────────────────────────────────────────
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2).IsRequired();

                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Payment ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Status)
                      .HasConversion<string>()
                      .HasMaxLength(32);

                entity.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(p => p.PreferenceId).HasMaxLength(256).IsRequired();

                // Índice en PreferenceId porque cuando MercadoPago nos llame
                // en el webhook, vamos a necesitar encontrar el pago por este ID
                // muy rápidamente — es una operación que ocurre en tiempo real
                // mientras el usuario espera la confirmación de su compra.
                entity.HasIndex(p => p.PreferenceId);
                entity.Property(p => p.MercadoPagoPaymentId).HasMaxLength(256);
            });
        }
    }
}
