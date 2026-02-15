using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Orders.GetOrderById
{
    [Route("orders")]
    public class GetOrderByIdEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpGet("{id:guid}")]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle(Guid id)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            var order = await db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order is null)
                return NotFound(new { message = "Orden no encontrada" });

            var response = new OrderDetailResponse(
                order.Id,
                order.Status.ToString(),
                order.Total,
                order.Items.Select(i => new OrderItemDetailResponse(
                    i.Id, i.ProductId, i.Product.Name,
                    i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity
                )),
                order.Payment?.Status.ToString(),
                // El init_point no lo guardamos en la DB — el cliente tiene que
                // llamar a POST /orders/{id}/payment para obtenerlo si lo necesita.
                // Esto es intencional: las preferencias de MercadoPago tienen su
                // propio tiempo de expiración y crear una nueva es barato.
                null,
                order.CreatedAt,
                order.UpdatedAt
            );

            return Ok(response);
        }
    }
}
