using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Products.GetProductById
{
    [Route("products")]
    public class GetProductByIdEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Handle(Guid id)
        {
            var product = await db.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new ProductDetailResponse(
                    p.Id, p.Name, p.Description, p.Price,
                    p.Stock, p.ImageUrl, p.IsActive,
                    p.CategoryId, p.Category.Name,
                    p.CreatedAt, p.UpdatedAt
                ))
                .FirstOrDefaultAsync();

            if (product is null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }
    }
}
