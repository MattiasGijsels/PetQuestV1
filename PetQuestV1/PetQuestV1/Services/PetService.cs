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
            return _petRepository.AddAsync(pet);
        }

        public Task UpdateAsync(Pet pet)
        {
            return _petRepository.UpdateAsync(pet);
        }

        public Task DeleteAsync(string id)
        {
            return _petRepository.DeleteAsync(id);
        }
        public Task<Species?> GetSpeciesByNameAsync(string name) // Implement this method
        {
            return _petRepository.GetSpeciesByNameAsync(name);
        }
    }
}
