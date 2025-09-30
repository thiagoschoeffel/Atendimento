using Atendimento.Api.Data;
using Atendimento.Api.Models.Auth;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Atendimento.Api.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator(AppDbContext db)
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Informe o usuário.")
                .MinimumLength(3).WithMessage("Usuário deve ter pelo menos 3 caracteres.")
                .MaximumLength(100).WithMessage("Usuário deve ter no máximo 100 caracteres.")
                .MustAsync(async (username, ct) =>
                    !await db.Users.AsNoTracking().AnyAsync(u => u.Username == username, ct))
                .WithMessage("Usuário já existe.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Informe a senha.")
                .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres.")
                .Matches("[A-Z]").WithMessage("Senha deve conter ao menos 1 letra maiúscula.")
                .Matches("[a-z]").WithMessage("Senha deve conter ao menos 1 letra minúscula.")
                .Matches("[0-9]").WithMessage("Senha deve conter ao menos 1 dígito.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter ao menos 1 caractere especial.");
        }
    }
}
