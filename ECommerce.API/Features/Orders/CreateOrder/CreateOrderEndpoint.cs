using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Orders.CreateOrder
{
    [Route("orders")]
    public class CreateOrderEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle()
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            var cart = await db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart is null || !cart.Items.Any())
                return BadRequest(new { message = "El carrito está vacío" });

            var stockErrors = cart.Items
                .Where(i => i.Product.IsActive && i.Product.Stock < i.Quantity)
                .Select(i => $"{i.Product.Name}: disponible {i.Product.Stock}, pedido {i.Quantity}")
                .ToList();

            if (stockErrors.Any())
                return BadRequest(new
                {
                    message = "Stock insuficiente para algunos productos",
                    errors = stockErrors
                });

            var activeItems = cart.Items.Where(i => i.Product.IsActive).ToList();

            if (!activeItems.Any())
                return BadRequest(new { message = "Todos los productos del carrito fueron desactivados" });

            // Usamos CreateExecutionStrategy para que la estrategia de reintentos sepa
            // exactamente qué bloque de código repetir si ocurre un error transitorio.
            // Sin esto, EF Core no puede reintentar de forma segura dentro de una transacción
            // porque no sabe cuánto trabajo ya se hizo antes del fallo.
            CreateOrderResponse? response = null;

            var executionStrategy = db.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();

                try
                {
                    var total = activeItems.Sum(i => i.Product.Price * i.Quantity);

                    var order = new Order
                    {
                        UserId = user.Id,
                        Total = total,
                        Status = OrderStatus.Pending
                    };

                    db.Orders.Add(order);
                    await db.SaveChangesAsync();

                    var orderItems = activeItems.Select(i => new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.Product.Price
                    }).ToList();

                    db.OrderItems.AddRange(orderItems);

                    foreach (var item in activeItems)
                        item.Product.Stock -= item.Quantity;

                    db.CartItems.RemoveRange(cart.Items);
                    cart.UpdatedAt = DateTime.UtcNow;

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    response = new CreateOrderResponse(
                        order.Id,
                        order.Status.ToString(),
                        order.Total,
                        orderItems.Select((oi, index) => new OrderItemResponse(
                            oi.Id,
                            oi.ProductId,
                            activeItems[index].Product.Name,
                            oi.Quantity,
                            oi.UnitPrice,
                            oi.UnitPrice * oi.Quantity
                        )),
                        order.CreatedAt
                    );
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; // re-lanzamos para que la estrategia de reintentos lo maneje
                }
            });

            if (response is null)
                return StatusCode(500, new { message = "Error al procesar la orden" });

            return CreatedAtAction(nameof(Handle), new { id = response.Id }, response);
        }
    }
}