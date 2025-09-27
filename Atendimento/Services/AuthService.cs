using Atendimento.Data;
using Atendimento.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Atendimento.Services
{
    public interface IAuthService
    {
        Task<User?> ValidateUserAsync(string username, string password, CancellationToken ct);
    }

    public class AuthService(AppDbContext db, IPasswordHasher<object> hasher) : IAuthService
    {
        private static readonly object _scope = new();

        public async Task<User?> ValidateUserAsync(string username, string password, CancellationToken ct)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, ct);
            if (user is null) return null;

            var result = hasher.VerifyHashedPassword(_scope, user.PasswordHash, password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded
                ? user
                : null;
        }
    }
}
