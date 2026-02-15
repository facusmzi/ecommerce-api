using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Features.Cart.UpdateItem
{
    public record UpdateItemRequest(
    [Range(1, 100)] int Quantity
    );
}
