// PetQuestV1/Services/SpeciesService.cs
using PetQuestV1.Data.Defines; // For ISpeciesRepository
using PetQuestV1.Contracts.Defines; // For ISpeciesService
using PetQuestV1.Contracts.Models; // For Species model
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Services
{
    public class SpeciesService : ISpeciesService
    {
        private readonly ISpeciesRepository _speciesRepository;

        public SpeciesService(ISpeciesRepository speciesRepository)
        {
            _speciesRepository = speciesRepository;
        }

        public Task<List<Species>> GetAllAsync()
        {
            return _speciesRepository.GetAllAsync();
        }

        public Task<Species?> GetByIdAsync(string id)
        {
            return _speciesRepository.GetByIdAsync(id);
        }

        public Task AddAsync(Species species)
        {
            // Add any business logic/validation here before calling the repository
            return _speciesRepository.AddAsync(species);
        }

        public Task UpdateAsync(Species species)
        {
            // Add any business logic/validation here before calling the repository
            return _speciesRepository.UpdateAsync(species);
        }

        public async Task SoftDeleteAsync(string id)
        {
            // You could fetch the species first to perform additional checks if needed
            // var species = await _speciesRepository.GetByIdAsync(id);
            // if (species != null) { ... }
            await _speciesRepository.SoftDeleteAsync(id);
        }
    }
}