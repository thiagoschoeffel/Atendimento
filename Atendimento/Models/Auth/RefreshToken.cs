using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atendimento.Models.Auth
{
    public class RefreshToken
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        [Required] public string Token { get; set; } = default!;
        [Required] public Guid UserId { get; set; }
        public Atendimento.Models.User User { get; set; } = default!;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        [NotMapped] public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
        [NotMapped] public bool IsActive => RevokedAtUtc is null && !IsExpired;
    }
}
