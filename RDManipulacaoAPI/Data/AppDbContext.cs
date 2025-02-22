using Microsoft.EntityFrameworkCore;
using RDManipulacaoAPI.Models;

namespace RDManipulacaoAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Video> Videos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Video>().Property(v => v.Titulo)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}
