using FashionShop.Application.Cart.Commands;
using FashionShop.Application.Cart.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ISender _mediator;
        public CartController(ISender mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetCart()
        {
            var result = await _mediator.Send(new GetCartQuery());
            return Ok(result);
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddItemToCartCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveItemFromCartCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

    }
}
