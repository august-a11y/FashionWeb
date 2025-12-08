using FashionShop.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController
    {

        private readonly ISender _mediator;
        public CategoryController(ISender mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] IRequest<int> command)
        {
            var id = await _mediator.Send(command);
            return new OkObjectResult(id);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _mediator.Send(new GetCategoryQuery());
            return new OkObjectResult(result);
        }

    }
}
