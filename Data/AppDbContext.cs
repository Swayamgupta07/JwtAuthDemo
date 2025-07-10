using JwtAuthDemo.Model.Entity;
using Microsoft.EntityFrameworkCore;
namespace JwtAuthDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {}

        public DbSet<AppUser> AppUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Email)
                      .IsRequired();

                entity.Property(e => e.Passwordhash)
                      .IsRequired();
            });
        }
    }
}