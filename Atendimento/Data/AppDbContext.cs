using Atendimento.Models;
using Atendimento.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace Atendimento.Data
{

    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.HasPostgresExtension("pgcrypto");
            b.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasIndex(u => u.Username).IsUnique();

                e.Property(u => u.Id)
                 .HasColumnType("uuid")
                 .HasDefaultValueSql("gen_random_uuid()")
                 .ValueGeneratedOnAdd();

                e.Property(u => u.Username).HasMaxLength(100).IsRequired();
                e.Property(u => u.PasswordHash).IsRequired();
            });

            b.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_tokens");
                e.Property(x => x.Id).HasColumnType("uuid")
                                     .HasDefaultValueSql("gen_random_uuid()")
                                     .ValueGeneratedOnAdd();
                e.Property(x => x.Token).IsRequired();
                e.HasIndex(x => x.Token).IsUnique();
                e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
