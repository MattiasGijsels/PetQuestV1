// PetQuestV1/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Models;
using System.Linq;

namespace PetQuestV1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<Breed> Breeds { get; set; } // Ensure DbSet for Breed is here

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // Configure Pet-Owner relationship
            builder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Pets)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Pet-Species relationship
            builder.Entity<Pet>()
                .HasOne(p => p.Species)
                .WithMany() // A Pet has one Species, a Species has many Pets (implied by this setup)
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);// cannot delete a Species if any Pet is using it, prevents accidental dataloss

            // Configure Pet-Breed relationship
            builder.Entity<Pet>()
                .HasOne(p => p.Breed)
                .WithMany() // A Pet has one Breed, a Breed has many Pets (implied by this setup)
                .HasForeignKey(p => p.BreedId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configures Species-Breed relationship - Keep CASCADE if you want this specific behavior
            // Deleting a Species will delete all its associated Breeds.
            builder.Entity<Species>()
                .HasMany(s => s.Breeds)
                .WithOne(b => b.Species)
                .HasForeignKey(b => b.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);


            // --- GLOBAL QUERY FILTER FOR SOFT DELETION ---
            builder.Entity<Pet>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Species>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<Breed>().HasQueryFilter(b => !b.IsDeleted);  
        }
    }
}