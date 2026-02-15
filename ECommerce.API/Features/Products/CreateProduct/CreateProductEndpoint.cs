using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Products.CreateProduct
{
    [Route("products")]
    public class CreateProductEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost]
        [Authorize]
        [RequireVerifiedEmail]
        [RequireAdmin]
        public async Task<IActionResult> Handle([FromBody] CreateProductRequest request)
        {
            // Verificamos que la categoría exista antes de crear el producto.
            // Sin esta validación, EF Core tiraría una excepción de foreign key
            // en SaveChangesAsync y el mensaje de error sería críptico para el cliente.
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
                return BadRequest(new { message = "La categoría especificada no existe" });

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                ImageUrl = request.ImageUrl,
                CategoryId = request.CategoryId
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            // Cargamos la categoría para incluir su nombre en la respuesta.
            // Lo hacemos después de guardar porque en este punto el producto
            // ya tiene su Id generado por la base de datos.
            await db.Entry(product).Reference(p => p.Category).LoadAsync();

            return CreatedAtAction(
                nameof(Handle),
                new { id = product.Id },
                new CreateProductResponse(
                    product.Id, product.Name, product.Price,
                    product.Stock, product.CategoryId,
                    product.Category.Name, product.CreatedAt
                )
            );
        }
    }
}
