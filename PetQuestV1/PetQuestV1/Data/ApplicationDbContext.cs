using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Models; // Assuming your models are here
using System.Linq; // Needed for .SelectMany()

namespace PetQuestV1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<Breed> Breeds { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // CALL THIS FIRST FOR IDENTITY TABLES

            // Important: Disable cascade delete for all relationships by default
            // This is a common practice to avoid unexpected cascade behavior and resolve cycles
            // You can then explicitly set Cascade where desired, like Species-Breeds.
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction; // Or .Restrict
            }

            // --- Configure Specific Relationships ---

            // Configures Pet-Owner relationship
            // An Owner cannot be deleted if they have any Pets.
            builder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configures Pet-Species relationship
            // A Species cannot be deleted if any Pets still reference it.
            builder.Entity<Pet>()
                .HasOne(p => p.Species)
                .WithMany()
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configures Pet-Breed relationship
            // A Breed cannot be deleted if any Pets still reference it.
            builder.Entity<Pet>()
                .HasOne(p => p.Breed)
                .WithMany()
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
            // This is the crucial part. It automatically filters out Pet entities
            // where IsDeleted is true from ALL queries, because Pet inherits IsDeleted
            // from ModelBase.
            builder.Entity<Pet>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);

            // If other models like Species or Breed also inherit from ModelBase
            // and you want them soft-deleted, you would add similar lines:
            // builder.Entity<Species>().HasQueryFilter(s => !s.IsDeleted);
            // builder.Entity<Breed>().HasQueryFilter(b => !b.IsDeleted);
        }
    }
}