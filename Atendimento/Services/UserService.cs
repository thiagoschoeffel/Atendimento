using Atendimento.Data;
using Atendimento.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atendimento.Services
{
    public interface IUserService
    {
        Task<User> CreateAsync(string username, string password, CancellationToken ct);
        Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
    }

    public class UserService(AppDbContext db, IPasswordHasher<object> hasher) : IUserService
    {
        private static readonly object _pwdScope = new();

        public async Task<User> CreateAsync(string username, string password, CancellationToken ct)
        {
            if (await db.Users.AnyAsync(u => u.Username == username, ct))
                throw new InvalidOperationException("username_already_exists");

            var hash = hasher.HashPassword(_pwdScope, password);
            var user = new User { Username = username, PasswordHash = hash };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
            return user;
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct) =>
            db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, ct);
    }
}
