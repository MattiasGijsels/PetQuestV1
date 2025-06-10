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
    public class BreedRepository : IBreedRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BreedRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Breed>> GetAllBreedsWithSpeciesAsync()
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.Breeds
                                     .Include(b => b.Species) 
                                     .ToListAsync();
            }
        }

        public async Task<Breed?> GetByIdAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                return await _context.Breeds

                                     .FirstOrDefaultAsync(b => b.Id == id);
            }
        }

        public async Task AddAsync(Breed breed)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                if (string.IsNullOrEmpty(breed.Id))
                {
                    breed.Id = Guid.NewGuid().ToString("N");
                }
                breed.IsDeleted = false;
                await _context.Breeds.AddAsync(breed);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Breed breed)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var existingBreed = await _context.Breeds.FindAsync(breed.Id);
                if (existingBreed != null)
                {
                    existingBreed.BreedName = breed.BreedName;
                    existingBreed.SpeciesId = breed.SpeciesId; 
                    _context.Breeds.Update(existingBreed);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task SoftDeleteAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var breed = await _context.Breeds.FindAsync(id);
                if (breed != null)
                {
                    breed.IsDeleted = true;
                    _context.Breeds.Update(breed);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task HardDeleteAsync(string id)
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var breed = await _context.Breeds
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (breed != null)
                {
                    _context.Breeds.Remove(breed);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}