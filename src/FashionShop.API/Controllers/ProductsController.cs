using FashionShop.Application.Dtos;
using FashionShop.Application.Products.Commands;
using FashionShop.Application.Products.Queries;
using FashionShop.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/products")]
    
    public class ProductsController : ControllerBase
    {

        private readonly ISender _mediator;
        public ProductsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        //[Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _mediator.Send(new GetAllProductQuery());
            return Ok(result);
        }
        [Authorize(Policy = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(id);
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        [HttpPost("getById")]
        public async Task<IActionResult> GetProductById([FromBody] GetProductByIdQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpPost("getList")]

        public async Task<IActionResult> GetListProduct([FromBody] GetListProductQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
