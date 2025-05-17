using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Species> Species { get; set; } 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Call the base implementation FIRST

            // Configures Pet-Owner relationship
            builder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId);

            // Configures Pet-Species relationship
            builder.Entity<Pet>()
                .HasOne(p => p.Species)
                .WithMany()
                .HasForeignKey(p => p.SpeciesId);
        }
    }
}
