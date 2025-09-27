using Atendimento.Models;
using Microsoft.EntityFrameworkCore;

namespace Atendimento.Data
{

    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

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
        }
    }
}
