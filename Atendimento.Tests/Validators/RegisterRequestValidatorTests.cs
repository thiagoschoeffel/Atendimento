using Atendimento.Data;
using Atendimento.Models;
using Atendimento.Models.Auth;
using Atendimento.Validators;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;

namespace Atendimento.Tests.Validators
{
    public class RegisterRequestValidatorTests
    {
        private static AppDbContext MakeDb()
        {
            var opt = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            var db = new AppDbContext(opt);
            db.Database.OpenConnection();
            db.Database.EnsureCreated();
            return db;
        }

        [Fact]
        public async Task Username_deve_ser_unico()
        {
            using var db = MakeDb();
            db.Users.Add(new User
            {
                Username = "joao",
                PasswordHash = "x"
            });
            await db.SaveChangesAsync();

            var validator = new RegisterRequestValidator(db);
            var req = new RegisterRequest
            {
                Username = "joao",
                Password = "Password1!"
            };

            var result = await validator.TestValidateAsync(req);
            result.ShouldHaveValidationErrorFor(r => r.Username)
                  .WithErrorMessage("Usuário já existe.");
        }

        [Fact]
        public async Task Password_deve_atender_complexidade_minima()
        {
            using var db = MakeDb();
            var validator = new RegisterRequestValidator(db);

            var req = new RegisterRequest
            {
                Username = "novo",
                Password = "abc"
            };

            var result = await validator.TestValidateAsync(req);
            result.ShouldHaveValidationErrorFor(r => r.Password);
        }
    }
}
