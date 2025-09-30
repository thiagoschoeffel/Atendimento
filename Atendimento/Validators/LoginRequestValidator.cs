using Atendimento.Models.Auth;
using FluentValidation;

namespace Atendimento.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Informe o usuário.")
                .MinimumLength(3).WithMessage("Usuário deve ter pelo menos 3 caracteres.")
                .MaximumLength(100).WithMessage("Usuário deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Informe a senha.")
                .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres.");
        }
    }
}
