using FashionShop.Application.User.Commands;
using FashionShop.Application.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace FashionShop.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ISender _sender;
        public AuthenticationController(ISender sender)
        {
            _sender = sender;
        }
        [HttpPost("login")]
        public async Task<IActionResult<AuthenticatedResult>> Login([FromBody] LoginUserCommand query)
        {
            var result = await _sender.Send(query);
            if(result == null)
            {
                return Unauthorized("Invalid UserName or PassWord");
            }
            return new OkObjectResult(result);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserCommand command)
        {
            var result = await _sender.Send(command);
            if (!result)
            {
                return BadRequest("Register Fail");
            }
            return new OkObjectResult(result);

        }
    }
}
