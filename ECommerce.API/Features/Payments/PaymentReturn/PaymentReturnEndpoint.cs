using ECommerce.API.Common.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Payments.PaymentReturn
{
    [ApiController]
    [Route("payments/return")]
    public class PaymentReturnEndpoint(AppDbContext db) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Handle(
            [FromQuery] string? status,
            [FromQuery(Name = "external_reference")] string? orderId,
            [FromQuery(Name = "payment_id")] string? paymentId)
        {
            if (string.IsNullOrEmpty(orderId))
                return Ok(new { message = "Pago procesado", status });

            if (!Guid.TryParse(orderId, out var orderGuid))
                return BadRequest(new { message = "Order ID inválido" });

            var order = await db.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderGuid);

            if (order is null)
                return NotFound(new { message = "Orden no encontrada" });

            return Ok(new
            {
                message = "Pago procesado",
                orderId = order.Id,
                orderStatus = order.Status.ToString(),
                paymentStatus = order.Payment?.Status.ToString(),
                mpStatus = status,
                mpPaymentId = paymentId
            });
        }
    }
}
