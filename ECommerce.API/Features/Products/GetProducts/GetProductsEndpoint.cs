using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Products.GetProducts
{
    [Route("products")]
    public class GetProductsEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpGet]
        public async Task<IActionResult> Handle([FromQuery] GetProductsRequest request)
        {
            // Arrancamos con un IQueryable — esto es importante porque EF Core
            // no ejecuta ninguna consulta SQL hasta que llamemos a ToListAsync()
            // al final. Todo lo que hacemos acá es construir la query en memoria
            // y EF Core la traduce a SQL una sola vez al final.
            // Esto se llama "deferred execution" y es lo que hace eficiente
            // encadenar múltiples filtros sin hacer múltiples viajes a la DB.
            var query = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)  // nunca mostramos productos inactivos en el catálogo
                .AsQueryable();

            // Aplicamos el filtro de categoría solo si el cliente lo pidió.
            // El patrón "if tiene valor, filtrá" es el estándar para filtros opcionales.
            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);

            // Búsqueda de texto — buscamos en nombre y descripción.
            // EF Core traduce esto a un LIKE en SQL Server.
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.Description != null && p.Description.ToLower().Contains(search))
                );
            }

            // Contamos el total ANTES de paginar — así podemos calcular TotalPages
            // y el cliente sabe cuántas páginas hay en total.
            var totalItems = await query.CountAsync();

            // Paginación: Skip salta los registros de las páginas anteriores,
            // Take limita la cantidad de registros de esta página.
            // Por ejemplo para page=3, pageSize=20: skip 40, take 20.
            var pageSize = Math.Clamp(request.PageSize, 1, 100); // máximo 100 por página
            var page = Math.Max(request.Page, 1);

            var items = await query
                .OrderByDescending(p => p.CreatedAt) // más recientes primero
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponse(
                    p.Id, p.Name, p.Description, p.Price,
                    p.Stock, p.ImageUrl, p.IsActive,
                    p.CategoryId, p.Category.Name,
                    p.CreatedAt, p.UpdatedAt
                ))
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return Ok(new PagedResponse<ProductResponse>(
                items, totalItems, page, pageSize, totalPages
            ));
        }
    }
}
