namespace Atendimento.Api.Models.Auth
{
    public sealed class LoginRequest
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
