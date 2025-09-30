namespace Atendimento.Api.Models.Auth
{
    public sealed class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
