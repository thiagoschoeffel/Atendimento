using Atendimento.Models.Auth;
using Atendimento.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Atendimento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IConfiguration configuration, IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var user = await authService.ValidateUserAsync(request.Username, request.Password, ct);
            if (user is null) return Unauthorized(new { error = "invalid_credentials" });

            var token = GenerateJwtToken(user.Username, out var exp);
            return Ok(new AuthResponse { AccessToken = token, ExpiresAtUtc = exp });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var name = User.Identity?.Name ?? "(unknown)";
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
            return Ok(new { user = name, roles });
        }

        private string GenerateJwtToken(string username, out DateTime expiresUtc)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var key = jwtSection["Key"];
            var expirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "admin")
        };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            expiresUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresUtc,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
