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
            return await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .ToListAsync();
        }

        public async Task<Pet?> GetByIdAsync(string id)
        {
            return await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Pet>> GetPetsByOwnerIdAsync(string ownerId)
        {
            return await _context.Pets
                .Where(p => p.OwnerId == ownerId)
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .ToListAsync();
        }

        public async Task AddAsync(Pet pet)
        {
            if (string.IsNullOrEmpty(pet.Id))
            {
                pet.Id = Guid.NewGuid().ToString("N");
            }
            pet.IsDeleted = false;
            await _context.Pets.AddAsync(pet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pet pet)
        {
            var existingPet = await _context.Pets.FindAsync(pet.Id);
            if (existingPet != null)
            {
                existingPet.PetName = pet.PetName;
                existingPet.SpeciesId = pet.SpeciesId;
                existingPet.BreedId = pet.BreedId;
                existingPet.Age = pet.Age;
                existingPet.OwnerId = pet.OwnerId;
                existingPet.ImagePath = pet.ImagePath;
                existingPet.Advantage = pet.Advantage;
                existingPet.IsDeleted = pet.IsDeleted;

                _context.Pets.Update(existingPet);
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
        public async Task<List<Pet>> GetAllPetsWithDetailsAsync()
        {
            return await _context.Pets
                                 .Include(p => p.Species) 
                                 .Include(p => p.Breed)  
                                 .Include(p => p.Owner) 
                                 .Where(p => !p.IsDeleted)
                                 .ToListAsync();
        }
        public async Task<Pet?> GetPetWithDetailsByIdAsync(string id)
        {
            return await _context.Pets
                                 .Include(p => p.Species)
                                 .Include(p => p.Breed)
                                 .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }
    }
}