using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Cart.AddItem
{
    [Route("cart/items")]
    public class AddItemEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle([FromBody] AddItemRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            // Verificamos que el producto exista, esté activo, y tenga stock suficiente.
            // Hacemos todo esto antes de tocar el carrito para fallar rápido si algo está mal.
            var product = await db.Products.FindAsync(request.ProductId);

            if (product is null || !product.IsActive)
                return NotFound(new { message = "Producto no encontrado" });

            if (product.Stock < request.Quantity)
                return BadRequest(new { message = $"Stock insuficiente. Disponible: {product.Stock}" });

            // Patrón lazy: buscamos el carrito, y si no existe lo creamos ahora.
            // Usamos Include para cargar los items existentes en la misma consulta
            // y evitar hacer dos viajes a la base de datos.
            var cart = await db.Carts
    .Include(c => c.Items)
    .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Si no existe el carrito, lo creamos y guardamos PRIMERO antes de
            // agregar items. Esto evita el DbUpdateConcurrencyException que ocurre
            // cuando EF Core intenta hacer INSERT y UPDATE en el mismo SaveChangesAsync.
            if (cart is null)
            {
                cart = new Common.Models.Cart { UserId = user.Id };
                db.Carts.Add(cart);
                await db.SaveChangesAsync(); // guardamos el carrito para que tenga ID en DB
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

            if (existingItem is not null)
            {
                var newQuantity = existingItem.Quantity + request.Quantity;

                if (product.Stock < newQuantity)
                    return BadRequest(new
                    {
                        message = $"Stock insuficiente. Disponible: {product.Stock}, ya tenés {existingItem.Quantity} en el carrito"
                    });

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                db.CartItems.Add(item); // usamos db.CartItems.Add en lugar de cart.Items.Add
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Ok(new { message = "Producto agregado al carrito" });
        }
    }
}
