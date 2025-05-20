// Repositories/PetRepository.cs
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using System;
using System.Collections.Generic;
using System.Linq; // Add this for .Where()
using System.Threading.Tasks;

namespace PetQuestV1.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly ApplicationDbContext _context;

        public PetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pet>> GetAllAsync()
        {
            return await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .Include(p => p.Breed) // --- NEW: Include Breed ---
                .ToListAsync();
        }

        public async Task<Pet?> GetByIdAsync(string id)
        {
            return await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .Include(p => p.Breed) // --- NEW: Include Breed ---
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Pet pet)
        {
            if (string.IsNullOrEmpty(pet.Id)) // Ensure ID is generated if not set
            {
                pet.Id = Guid.NewGuid().ToString();
            }
            await _context.Pets.AddAsync(pet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pet pet)
        {
            // When updating, you generally fetch the entity and then update its scalar properties
            // or attach it to the context and mark as modified.
            // Using CurrentValues.SetValues(pet) is an option but might be tricky with navigation properties.
            // A more explicit way for updating, especially with FKs:
            var existingPet = await _context.Pets.FindAsync(pet.Id);
            if (existingPet != null)
            {
                existingPet.PetName = pet.PetName;
                existingPet.SpeciesId = pet.SpeciesId;
                existingPet.BreedId = pet.BreedId; // --- NEW: Update BreedId ---
                existingPet.Age = pet.Age;
                existingPet.OwnerId = pet.OwnerId;
                // Add any other scalar properties you want to update

                _context.Pets.Update(existingPet); // Mark as modified
                await _context.SaveChangesAsync();
            }
        }

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

        // --- NEW: Implement Breed methods ---
        public async Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId)
        {
            if (string.IsNullOrEmpty(speciesId))
            {
                return new List<Breed>(); // Return empty list if no species ID is provided
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