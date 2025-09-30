using Atendimento.Api.Models.Auth;
using Atendimento.Api.Validators;
using FluentValidation.TestHelper;

namespace Atendimento.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        [Fact]
        public async Task Username_e_password_sao_obrigatorios()
        {
            var validator = new LoginRequestValidator();

            var req = new LoginRequest
            {
                Username = "",
                Password = ""
            };
            var result = await validator.TestValidateAsync(req);

            result.ShouldHaveValidationErrorFor(r => r.Username);
            result.ShouldHaveValidationErrorFor(r => r.Password);
        }
    }
}
