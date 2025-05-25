// PetQuestV1/Data/Repository/SpeciesRepository.cs
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Data.Defines; // For ISpeciesRepository
using PetQuestV1.Contracts.Models; // For Species model
using PetQuestV1.Data; // For ApplicationDbContext
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Data.Repository
{
    public class SpeciesRepository : ISpeciesRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public SpeciesRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Species>> GetAllAsync()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                // The global query filter in ApplicationDbContext will automatically exclude IsDeleted species.
                return await _context.Species.ToListAsync();
            }
        }

        public async Task<Species?> GetByIdAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                // The global query filter will automatically exclude IsDeleted species.
                return await _context.Species.FirstOrDefaultAsync(s => s.Id == id);
            }
        }

        public async Task AddAsync(Species species)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(species.Id))
                {
                    species.Id = Guid.NewGuid().ToString("N"); // "N" format for no hyphens
                }
                species.IsDeleted = false; // Ensure new species are not marked as deleted
                await _context.Species.AddAsync(species);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Species species)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var existingSpecies = await _context.Species.FindAsync(species.Id);
                if (existingSpecies != null)
                {
                    existingSpecies.SpeciesName = species.SpeciesName;
                    // Note: We don't typically update IsDeleted directly via a general Update method
                    // unless explicitly intended. SoftDeleteAsync handles setting IsDeleted = true.
                    // If you want to allow "undelete", you'd expose a specific method for it.
                    // For now, we'll assume Update is for non-deletion fields.
                    // existingSpecies.IsDeleted = species.IsDeleted; // Keep this line if you want to support undelete via this method.

                    _context.Species.Update(existingSpecies); // Mark as modified
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task SoftDeleteAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var species = await _context.Species.FindAsync(id);
                if (species != null)
                {
                    species.IsDeleted = true; // Set the IsDeleted flag
                    _context.Species.Update(species); // Mark as modified
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task HardDeleteAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var species = await _context.Species
                    .IgnoreQueryFilters() // Important: Ignore soft delete filter to find potentially soft-deleted entities for hard delete
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (species != null)
                {
                    _context.Species.Remove(species);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}