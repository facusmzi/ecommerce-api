using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Categories.GetCategories
{
    [Route("categories")]
    public class GetCategoriesEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpGet]
        public async Task<IActionResult> Handle()
        {
            var categories = await db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryResponse(
                    c.Id,
                    c.Name,
                    c.Description,
                    c.CreatedAt
                ))
                .ToListAsync();

            return Ok(categories);
        }
    }
}
