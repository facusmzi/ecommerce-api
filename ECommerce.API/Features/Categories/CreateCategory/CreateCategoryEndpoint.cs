using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Categories.CreateCategory
{
    [Route("categories")]
    public class CreateCategoryEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost]
        [Authorize]
        [RequireVerifiedEmail]
        [RequireAdmin]
        public async Task<IActionResult> Handle([FromBody] CreateCategoryRequest request)
        {
            // Verificamos que no exista una categoría con el mismo nombre.
            // Hacemos la comparación case-insensitive para evitar duplicados
            // como "Electrónica" y "electrónica".
            var exists = await db.Categories
                .AnyAsync(c => c.Name.ToLower() == request.Name.ToLower());

            if (exists)
                return Conflict(new { message = $"Ya existe una categoría con el nombre '{request.Name}'" });

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(Handle),
                new { id = category.Id },
                new CreateCategoryResponse(
                    category.Id,
                    category.Name,
                    category.Description,
                    category.CreatedAt
                )
            );
        }
    }
}
