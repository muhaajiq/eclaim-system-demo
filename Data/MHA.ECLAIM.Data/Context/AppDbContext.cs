using MHA.ECLAIM.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace MHA.ECLAIM.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<MainClaimHeader> MainClaimHeaders { get; set; }
        public DbSet<SubClaimDetails> SubClaimDetails { get; set; }
        public DbSet<TravelRequestHeaderEntity> TravelRequestHeader { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubClaimDetails>()
                .Property(p => p.ExchangeRate)
                .HasColumnType("decimal(18,6)");
        }
    }
}
