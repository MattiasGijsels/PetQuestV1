using Microsoft.EntityFrameworkCore;
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Data;
using System;
using System.Collections.Generic;
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
                .ToListAsync();
        }

        public async Task<Pet?> GetByIdAsync(string id)
        {
            return await _context.Pets
                .Include(p => p.Owner)
                .Include(p => p.Species)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Pet pet)
        {
            pet.Id = Guid.NewGuid().ToString();
            await _context.Pets.AddAsync(pet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pet pet)
        {
            var existingPet = await _context.Pets.FindAsync(pet.Id);
            if (existingPet != null)
            {
                _context.Entry(existingPet).CurrentValues.SetValues(pet);
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
    }
}
