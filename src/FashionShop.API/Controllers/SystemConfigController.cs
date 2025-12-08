using FashionShop.Application.Products.Commands;
using FashionShop.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SystemConfigController : ControllerBase
    {
        private readonly ISender _mediator;
        public SystemConfigController(ISender mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            var result = await _mediator.Send(new GetSystemConfigQuery());
            return Task.FromResult(Ok(result)).Result;
        }
        //public async Task<IActionResult> GetConfigByKey(string key)
        //{
        //    var result = await _mediator.Send(new GetSystemConfigByKeyQuery(key));
        //    return Task.FromResult(Ok(result)).Result;
        //}
        [HttpPost]
        public async Task<IActionResult> CreateConfig([FromBody] CreateSystemConfigCommand command)
        { 
            var result = await _mediator.Send(command);
            return Task.FromResult(Ok(result)).Result;
        }
    }
}
