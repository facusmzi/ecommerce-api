using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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
        [FromQuery] string? id,
        [FromBody] MercadoPagoWebhookBody? body)
    {
        var eventType = type ?? body?.Type;
        var paymentId = id ?? body?.Data?.Id;

        if (eventType != "payment" || string.IsNullOrEmpty(paymentId))
            return Ok();

        try
        {
            if (environment.IsDevelopment() && paymentId.StartsWith("test_"))
            {
                var testOrderId = paymentId["test_".Length..];
                if (!Guid.TryParse(testOrderId, out var testOrderGuid))
                    return Ok();

                return await ProcessOrderPayment(testOrderGuid, $"test_{Guid.NewGuid()}", "approved");
            }

            MercadoPagoConfig.AccessToken = configuration["MercadoPago:AccessToken"]!;
            var paymentClient = new PaymentClient();
            var mpPayment = await paymentClient.GetAsync(long.Parse(paymentId));

            if (mpPayment is null) return Ok();

            logger.LogInformation(
                "Webhook — Id: {Id}, Status: {Status}, ExternalRef: {Ref}",
                mpPayment.Id, mpPayment.Status, mpPayment.ExternalReference);

            if (!Guid.TryParse(mpPayment.ExternalReference, out var orderId))
                return Ok();

            return await ProcessOrderPayment(orderId, mpPayment.Id.ToString()!, mpPayment.Status!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error procesando webhook para id {Id}", paymentId);
        }

        return Ok();
    }

    private async Task<IActionResult> ProcessOrderPayment(Guid orderId, string mpPaymentId, string status)
    {
        var order = await db.Orders
            .Include(o => o.Payment)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null || order.Payment is null) return Ok();

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

        logger.LogInformation("Orden {OrderId} actualizada a {Status}", orderId, order.Status);
        return Ok();
    }
}

public class MercadoPagoWebhookBody
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("data")]
    public MercadoPagoWebhookData? Data { get; set; }
}

public class MercadoPagoWebhookData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}