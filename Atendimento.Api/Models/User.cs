using System.ComponentModel.DataAnnotations;

namespace Atendimento.Api.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string Username { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;
    }
}
