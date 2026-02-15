using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Orders.GetOrders
{
    [Route("orders")]
    public class GetOrdersEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpGet]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle()
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            var orders = await db.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryResponse(
                    o.Id,
                    o.Status.ToString(),
                    o.Total,
                    o.Items.Sum(i => i.Quantity),
                    o.CreatedAt
                ))
                .ToListAsync();

            return Ok(orders);
        }
    }
}
