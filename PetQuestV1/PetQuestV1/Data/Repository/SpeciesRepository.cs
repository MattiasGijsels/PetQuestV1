// PetQuestV1/Data/Repository/SpeciesRepository.cs
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Data.Defines;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
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

        // Renamed/Modified method to include breeds
        public async Task<List<Species>> GetAllSpeciesWithBreedsAsync() // <--- MODIFIED METHOD NAME
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                // The global query filter will automatically exclude IsDeleted species AND breeds.
                // So, s.Breeds will only contain non-deleted breeds.
                return await _context.Species
                                     .Include(s => s.Breeds) // Include the navigation property
                                     .ToListAsync();
            }
        }

        public async Task<Species?> GetByIdAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                // Include breeds here too if you need them when fetching a single species
                return await _context.Species
                                     //.Include(s => s.Breeds) // Include breeds
                                     .FirstOrDefaultAsync(s => s.Id == id);
            }
        }

        public async Task AddAsync(Species species)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(species.Id))
                {
                    species.Id = Guid.NewGuid().ToString("N");
                }
                species.IsDeleted = false;
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
                    _context.Species.Update(existingSpecies);
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
                    species.IsDeleted = true;
                    _context.Species.Update(species);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task HardDeleteAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var species = await _context.Species
                    .IgnoreQueryFilters()
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