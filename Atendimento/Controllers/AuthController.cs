using Atendimento.Api.Data;
using Atendimento.Api.Models.Auth;
using Atendimento.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Atendimento.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService auth, ITokenService tokens, IConfiguration cfg) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            var user = await auth.ValidateUserAsync(req.Username, req.Password, ct);
            if (user is null) return Unauthorized(new { error = "invalid_credentials" });

            var access = tokens.GenerateAccessToken(user, out var accessExp);
            var days = cfg.GetValue<int>("Auth:RefreshTokenDays", 7);
            var refresh = tokens.NewRefresh(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), days);
            await tokens.SaveRefreshAsync(refresh, ct);

            SetRefreshCookie(refresh.Token, refresh.ExpiresAtUtc);
            return Ok(new AuthResponse { AccessToken = access, ExpiresAtUtc = accessExp });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var cookie = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(cookie)) return Unauthorized(new { error = "missing_refresh" });

            var (user, oldToken) = await tokens.GetActiveRefreshAsync(cookie, ct);

            var access = tokens.GenerateAccessToken(user, out var accessExp);
            var days = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<int>("Auth:RefreshTokenDays", 7);
            var newRefresh = tokens.NewRefresh(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString(), days);
            await tokens.RotateAsync(oldToken, newRefresh, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

            SetRefreshCookie(newRefresh.Token, newRefresh.ExpiresAtUtc);
            return Ok(new AuthResponse { AccessToken = access, ExpiresAtUtc = accessExp });
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke(CancellationToken ct)
        {
            var cookie = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(cookie)) return BadRequest(new { error = "missing_refresh" });

            var (_, oldToken) = await tokens.GetActiveRefreshAsync(cookie, ct);
            oldToken.RevokedAtUtc = DateTime.UtcNow;
            oldToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            await HttpContext.RequestServices.GetRequiredService<AppDbContext>().SaveChangesAsync(ct);
            Response.Cookies.Delete("refreshToken");
            return NoContent();
        }

        private void SetRefreshCookie(string token, DateTime expiresUtc)
        {
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expiresUtc,
                Path = "/"
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var name = User.Identity?.Name ?? "(unknown)";
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
            return Ok(new { user = name, roles });
        }
    }
}
