using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Cart.AddItem
{
    public record AddItemRequest(
    [Required] Guid ProductId,
    [Range(1, 100)] int Quantity = 1
    );
}
