using Atendimento.Data;
using Atendimento.Exceptions;
using Atendimento.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            var hash = hasher.HashPassword(_pwdScope, password);
            var user = new User { Username = username, PasswordHash = hash };

            db.Users.Add(user);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                                               pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new ConflictException("username_already_exists", "Usuário já existe.");
            }

            return user;
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct) =>
            db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, ct);
    }
}
