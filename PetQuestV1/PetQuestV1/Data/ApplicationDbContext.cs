using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Models;
using System.Linq;

namespace PetQuestV1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<Breed> Breeds { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            builder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Pets)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Pet>()
                .HasOne(p => p.Species)
                .WithMany() 
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Pet>()
                .HasOne(p => p.Breed)
                .WithMany() 
                .HasForeignKey(p => p.BreedId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Species>()
                .HasMany(s => s.Breeds)
                .WithOne(b => b.Species)
                .HasForeignKey(b => b.SpeciesId) 
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Pet>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Species>().HasQueryFilter(s => !s.IsDeleted);
            builder.Entity<Breed>().HasQueryFilter(b => !b.IsDeleted);  
        }
    }
}