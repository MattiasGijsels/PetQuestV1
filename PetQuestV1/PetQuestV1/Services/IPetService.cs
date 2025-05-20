// Contracts/IPetService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Contracts
{
    public interface IPetService
    {
        Task<List<Pet>> GetAllAsync();
        Task<Pet?> GetByIdAsync(string id);
        Task AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(string id);

        Task<Species?> GetSpeciesByNameAsync(string name); // This method seems to be only used by PetService, not the UI directly
        Task<List<Species>> GetAllSpeciesAsync();

        // --- NEW: Methods for Breed management ---
        Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId);
        Task<List<Breed>> GetAllBreedsAsync(); // Optional: if you need a list of ALL breeds
        Task<Breed?> GetBreedByIdAsync(string id); // Optional: if you need to fetch a single breed
    }
}