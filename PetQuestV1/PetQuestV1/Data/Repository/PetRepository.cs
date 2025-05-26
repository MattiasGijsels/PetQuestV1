// Repositories/PetRepository.cs
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using PetQuestV1.Data.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Data.Repository
{
    public class PetRepository(ApplicationDbContext context) : IPetRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<Pet>> GetAllAsync()
        {
            // The global query filter (see ApplicationDbContext) will automatically exclude IsDeleted pets.
            return await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .ToListAsync();
        }

        public async Task<Pet?> GetByIdAsync(string id)
        {
            // The global query filter will automatically exclude IsDeleted pets.
            // If you needed to retrieve a deleted pet, you'd use .IgnoreQueryFilters() here.
            return await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Pet pet)
        {
            if (string.IsNullOrEmpty(pet.Id))
            {
                pet.Id = Guid.NewGuid().ToString("N"); // "N" format for no hyphens
            }
            pet.IsDeleted = false; // Ensure new pets are not marked as deleted
            await _context.Pets.AddAsync(pet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pet pet)
        {
            // When updating, you generally fetch the entity and then update its scalar properties
            var existingPet = await _context.Pets.FindAsync(pet.Id);
            if (existingPet != null)
            {
                existingPet.PetName = pet.PetName;
                existingPet.SpeciesId = pet.SpeciesId;
                existingPet.BreedId = pet.BreedId;
                existingPet.Age = pet.Age;
                existingPet.OwnerId = pet.OwnerId;
                existingPet.ImagePath = pet.ImagePath;
                existingPet.IsDeleted = pet.IsDeleted; // Crucial: Update the IsDeleted flag

                _context.Pets.Update(existingPet); // Mark as modified
                await _context.SaveChangesAsync();
            }
        }

        // This method will no longer be directly used for soft deletion.
        // It remains here if you have a separate use case for permanent deletion.
        public async Task DeleteAsync(string id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Species?> GetSpeciesByNameAsync(string name)
        {
            return await _context.Species.FirstOrDefaultAsync(s => s.SpeciesName == name);
        }

        public async Task<List<Species>> GetAllSpeciesAsync()
        {
            return await _context.Species.ToListAsync();
        }

        public async Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId)
        {
            if (string.IsNullOrEmpty(speciesId))
            {
                return new List<Breed>();
            }
            return await _context.Breeds
                                 .Where(b => b.SpeciesId == speciesId)
                                 .OrderBy(b => b.BreedName)
                                 .ToListAsync();
        }

        public async Task<List<Breed>> GetAllBreedsAsync()
        {
            return await _context.Breeds.OrderBy(b => b.BreedName).ToListAsync();
        }

        public async Task<Breed?> GetBreedByIdAsync(string id)
        {
            return await _context.Breeds.FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}