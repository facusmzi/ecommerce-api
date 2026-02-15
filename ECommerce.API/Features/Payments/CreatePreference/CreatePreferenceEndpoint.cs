using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Payments.CreatePreference
{
    [Route("orders")]
    public class CreatePreferenceEndpoint(
        AppDbContext db,
        IJwtService jwtService,
        IConfiguration configuration,
        IWebHostEnvironment environment)
        : BaseEndpoint(db, jwtService)
    {
        [HttpPost("{orderId:guid}/payment")]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle(Guid orderId)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            var order = await db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

            if (order is null)
                return NotFound(new { message = "Orden no encontrada" });

            if (order.Status != OrderStatus.Pending)
                return BadRequest(new { message = "Esta orden ya fue procesada" });

            MercadoPagoConfig.AccessToken = configuration["MercadoPago:AccessToken"]!;

            var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var apiBaseUrl = configuration["Api:BaseUrl"]!;

            var preferenceItems = order.Items.Select(i => new PreferenceItemRequest
            {
                Id = i.ProductId.ToString(),
                Title = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                CurrencyId = "ARS",
                PictureUrl = i.Product.ImageUrl
            }).ToList();

            // En desarrollo las BackUrls apuntan a nuestro propio endpoint /payments/return
            // expuesto a través de ngrok — así MercadoPago puede redirigir al usuario
            // a una URL pública real y el browser no entra en loop.
            // En producción apuntan al frontend real.
            PreferenceBackUrlsRequest backUrls;

            if (environment.IsDevelopment())
            {
                var returnBase = $"{apiBaseUrl}/payments/return";
                backUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{returnBase}?status=success&external_reference={order.Id}",
                    Failure = $"{returnBase}?status=failure&external_reference={order.Id}",
                    Pending = $"{returnBase}?status=pending&external_reference={order.Id}"
                };
            }
            else
            {
                backUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{frontendUrl}/orders/{order.Id}?status=success",
                    Failure = $"{frontendUrl}/orders/{order.Id}?status=failure",
                    Pending = $"{frontendUrl}/orders/{order.Id}?status=pending"
                };
            }

            var preferenceRequest = new PreferenceRequest
            {
                Items = preferenceItems,
                BackUrls = backUrls,
                AutoReturn = environment.IsDevelopment() ? null : "approved",
                ExternalReference = order.Id.ToString(),
                NotificationUrl = $"{apiBaseUrl}/webhooks/mercadopago"
            };

            var client = new PreferenceClient();
            var preference = await client.CreateAsync(preferenceRequest);

            if (order.Payment is null)
            {
                var payment = new Payment
                {
                    OrderId = order.Id,
                    PreferenceId = preference.Id,
                    Amount = order.Total,
                    Status = PaymentStatus.Pending
                };
                db.Payments.Add(payment);
            }
            else
            {
                order.Payment.PreferenceId = preference.Id;
                order.Payment.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            var initPoint = environment.IsDevelopment()
                ? preference.SandboxInitPoint
                : preference.InitPoint;

            return Ok(new
            {
                preferenceId = preference.Id,
                initPoint
            });
        }
    }
}