using FashionShop.Application.Order.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [Route("api/order")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ISender _mediator;
        public OrderController(ISender mediator)
        {
            _mediator = mediator;
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var result =  await _mediator.Send(command);
            return Ok(result);
        }
    }
}
