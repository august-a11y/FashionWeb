using FashionShop.Application.Categories.Commands;
using FashionShop.Application.Categories.Query;
using FashionShop.Application.Dtos;
using FashionShop.Application.Products.Commands;
using FashionShop.Application.Products.Queries;
using FashionShop.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    
    [ApiController]
    [Route("api/category")]
    [Authorize]
    public class CategoryController : ControllerBase
    {

        private readonly ISender _mediator;
        public CategoryController(ISender mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            var id = await _mediator.Send(command);
            return new OkObjectResult(id);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryCommand command)
        {
            var result = await _mediator.Send(command);
            return new OkObjectResult(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryCommand command)
        {
            var result = await _mediator.Send(command);
            return new OkObjectResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _mediator.Send(new GetListCategoryQuery());
            return new OkObjectResult(result);
        }

    }
}
