using Atendimento.Api.Models.Auth;
using Atendimento.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atendimento.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserService users) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            var user = await users.CreateAsync(req.Username, req.Password, ct);
            return Ok(new { user.Id, user.Username });
        }
    }
}
