// Services/PetService.cs
using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        public Task<List<Pet>> GetAllAsync()
        {
            return _petRepository.GetAllAsync();
        }

        public Task<Pet?> GetByIdAsync(string id)
        {
            return _petRepository.GetByIdAsync(id);
        }

        public Task AddAsync(Pet pet)
        {
            // You might add business logic here before calling the repository
            return _petRepository.AddAsync(pet);
        }

        public Task UpdateAsync(Pet pet)
        {
            // You might add business logic here before calling the repository
            return _petRepository.UpdateAsync(pet);
        }

        public Task DeleteAsync(string id)
        {
            // You might add business logic here before calling the repository
            return _petRepository.DeleteAsync(id);
        }

        public async Task SoftDeleteAsync(string id)
        {
            var pet = await _petRepository.GetByIdAsync(id);
            if (pet != null)
            {
                pet.IsDeleted = true; // Set the IsDeleted flag to true
                await _petRepository.UpdateAsync(pet); // Update the pet in the database
            }
        }

        public Task<Species?> GetSpeciesByNameAsync(string name)
        {
            return _petRepository.GetSpeciesByNameAsync(name);
        }

        public Task<List<Species>> GetAllSpeciesAsync()
        {
            return _petRepository.GetAllSpeciesAsync();
        }

        // --- NEW: Implement Breed methods ---
        public Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId)
        {
            return _petRepository.GetBreedsBySpeciesIdAsync(speciesId);
        }

        public Task<List<Breed>> GetAllBreedsAsync()
        {
            return _petRepository.GetAllBreedsAsync();
        }

        public Task<Breed?> GetBreedByIdAsync(string id)
        {
            return _petRepository.GetBreedByIdAsync(id);
        }
    }
}