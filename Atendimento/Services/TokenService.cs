using Atendimento.Api.Data;
using Atendimento.Api.Models;
using Atendimento.Api.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Atendimento.Api.Services
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string Key { get; set; } = default!;
        public int ExpirationMinutes { get; set; } = 60;
    }

    public interface ITokenService
    {
        string GenerateAccessToken(User user, out DateTime expiresUtc);
        RefreshToken NewRefresh(Guid userId, string? ip, int days);
        Task<(User user, RefreshToken token)> GetActiveRefreshAsync(string token, CancellationToken ct);
        Task SaveRefreshAsync(RefreshToken token, CancellationToken ct);
        Task RotateAsync(RefreshToken oldToken, RefreshToken newToken, string? ip, CancellationToken ct);
    }

    public class TokenService(AppDbContext db, IOptions<JwtOptions> jwt) : ITokenService
    {
        private readonly JwtOptions _jwt = jwt.Value;

        public string GenerateAccessToken(User user, out DateTime expUtc)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                SecurityAlgorithms.HmacSha256);

            expUtc = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);

            var jwt = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims,
                                           notBefore: DateTime.UtcNow,
                                           expires: expUtc,
                                           signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public async Task<(User user, RefreshToken token)> GetActiveRefreshAsync(string token, CancellationToken ct)
        {
            var rt = await db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Token == token, ct);
            if (rt is null || !rt.IsActive) throw new SecurityException("invalid_refresh");
            return (rt.User, rt);
        }

        public RefreshToken NewRefresh(Guid userId, string? ip, int days) => new()
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            CreatedByIp = ip,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(days)
        };

        public async Task RotateAsync(RefreshToken oldToken, RefreshToken newToken, string? ip, CancellationToken ct)
        {
            oldToken.RevokedAtUtc = DateTime.UtcNow;
            oldToken.RevokedByIp = ip;
            oldToken.ReplacedByToken = newToken.Token;
            db.RefreshTokens.Add(newToken);
            await db.SaveChangesAsync(ct);
        }

        public async Task SaveRefreshAsync(RefreshToken token, CancellationToken ct)
        {
            db.RefreshTokens.Add(token);
            await db.SaveChangesAsync(ct);
        }
    }
}
