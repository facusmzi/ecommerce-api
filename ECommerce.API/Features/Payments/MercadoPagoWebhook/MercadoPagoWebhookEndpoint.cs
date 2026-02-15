using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Payments.MercadoPagoWebhook;

[ApiController]
[Route("webhooks")]
public class MercadoPagoWebhookEndpoint(
    AppDbContext db,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    ILogger<MercadoPagoWebhookEndpoint> logger)
    : ControllerBase
{
    [HttpPost("mercadopago")]
    public async Task<IActionResult> Handle(
        [FromQuery] string? type,
        [FromQuery] string? id)
    {
        if (type != "payment" || string.IsNullOrEmpty(id))
            return Ok();

        try
        {
            // Modo de prueba: en desarrollo podés simular un pago aprobado
            // pasando type=payment&id=test_{orderId} sin necesitar la UI del sandbox.
            // Ejemplo: POST /webhooks/mercadopago?type=payment&id=test_3fa85f64-...
            if (environment.IsDevelopment() && id.StartsWith("test_"))
            {
                var testOrderId = id["test_".Length..];

                if (!Guid.TryParse(testOrderId, out var testOrderGuid))
                    return BadRequest(new { message = "Order ID inválido en modo test" });

                return await ProcessOrderPayment(
                    orderId: testOrderGuid,
                    mpPaymentId: $"test_{Guid.NewGuid()}",
                    status: "approved"
                );
            }

            // Flujo normal: consultamos la API de MercadoPago
            MercadoPagoConfig.AccessToken = configuration["MercadoPago:AccessToken"]!;
            var paymentClient = new PaymentClient();
            var mpPayment = await paymentClient.GetAsync(long.Parse(id));

            if (mpPayment is null) return Ok();

            if (!Guid.TryParse(mpPayment.ExternalReference, out var orderId))
                return Ok();

            return await ProcessOrderPayment(
                orderId: orderId,
                mpPaymentId: mpPayment.Id.ToString()!,
                status: mpPayment.Status!
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando webhook de MercadoPago para id {Id}", id);
        }

        return Ok();
    }

    private async Task<IActionResult> ProcessOrderPayment(
        Guid orderId,
        string mpPaymentId,
        string status)
    {
        var order = await db.Orders
            .Include(o => o.Payment)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null || order.Payment is null)
            return Ok();

        switch (status)
        {
            case "approved":
                order.Status = OrderStatus.Paid;
                order.Payment.Status = PaymentStatus.Approved;
                order.Payment.MercadoPagoPaymentId = mpPaymentId;
                break;

            case "rejected":
            case "cancelled":
                order.Status = OrderStatus.Cancelled;
                order.Payment.Status = PaymentStatus.Rejected;

                foreach (var item in order.Items)
                    item.Product.Stock += item.Quantity;
                break;

            case "refunded":
                order.Payment.Status = PaymentStatus.Refunded;
                break;

            default:
                return Ok();
        }

        order.UpdatedAt = DateTime.UtcNow;
        order.Payment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        logger.LogInformation(
            "Orden {OrderId} actualizada a {Status}",
            orderId, order.Status);

        return Ok();
    }
}