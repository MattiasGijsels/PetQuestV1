using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Models; // Assuming your models are here

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
            // KEEP THIS. It's a good general practice.
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction; // Or .Restrict
            }

            // Re-configure specific relationships

            // Configures Pet-Owner relationship
            // You had SetNull. Let's make it Restrict to simplify and avoid any more conflicts.
            // This means an Owner cannot be deleted if they have any Pets.
            builder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // <--- CHANGE TO RESTRICT

            // Configures Pet-Species relationship
            // THIS IS THE ONE CAUSING THE CURRENT ERROR.
            // We need to force it to NOT be a cascade action if Species-Breed IS a cascade.
            // Restrict means a Species cannot be deleted if any Pets still reference it.
            builder.Entity<Pet>()
                .HasOne(p => p.Species)
                .WithMany()
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict); // <--- CHANGE TO RESTRICT

            // Configures Pet-Breed relationship
            // Similarly, use Restrict here.
            // Restrict means a Breed cannot be deleted if any Pets still reference it.
            builder.Entity<Pet>()
                .HasOne(p => p.Breed)
                .WithMany()
                .HasForeignKey(p => p.BreedId)
                .OnDelete(DeleteBehavior.Restrict); // <--- CHANGE TO RESTRICT

            // Configures Species-Breed relationship - Keep CASCADE if you want this specific behavior
            // This means deleting a Species will delete all its associated Breeds.
            builder.Entity<Species>()
                .HasMany(s => s.Breeds)
                .WithOne(b => b.Species)
                .HasForeignKey(b => b.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade); // Keep this if this is your desired behavior
        }
    }
}