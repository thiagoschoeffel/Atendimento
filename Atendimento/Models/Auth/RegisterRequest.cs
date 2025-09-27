namespace Atendimento.Models.Auth
{
    public sealed class RegisterRequest
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
