using Atendimento.Api.Data;
using Atendimento.Api.Exceptions;
using Atendimento.Api.Services;
using Atendimento.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Atendimento.Tests.Services
{
    public class UserServiceTests
    {
        private static (AppDbContext db, SqliteConnection conn) MakeDb()
        {
            var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            var opt = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;

            var db = new AppDbContext(opt);
            db.Database.EnsureCreated();
            return (db, conn);
        }

        [Fact]
        public async Task CreateAsync_salva_usuario_com_hash()
        {
            var (db, conn) = MakeDb();
            await using var _ = conn;

            var svc = new UserService(db, new PasswordHasher<object>());
            var user = await svc.CreateAsync("maria", "Password1!", CancellationToken.None);

            Assert.Equal("maria", user.Username);
            Assert.False(string.IsNullOrEmpty(user.PasswordHash));
        }

        [Fact]
        public async Task CreateAsync_lanca_Conflict_quando_username_duplicado()
        {
            var (db, conn) = MakeDb();
            await using var _ = conn;

            db.Users.Add(new User
            {
                Username = "joao",
                PasswordHash = "x"
            });
            await db.SaveChangesAsync();

            var svc = new UserService(db, new PasswordHasher<object>());

            var ex = await Assert.ThrowsAsync<ConflictException>(() =>
                svc.CreateAsync("joao", "Password1!", CancellationToken.None));

            Assert.Equal("username_already_exists", ex.Code);
            Assert.Equal("Usuário já existe.", ex.Message);
        }
    }
}
