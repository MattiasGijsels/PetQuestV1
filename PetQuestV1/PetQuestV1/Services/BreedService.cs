// PetQuestV1/Services/BreedService.cs
using PetQuestV1.Data.Defines;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Services
{
    public class BreedService : IBreedService
    {
        private readonly IBreedRepository _breedRepository;
        private readonly ISpeciesRepository _speciesRepository; // Inject SpeciesRepository to get species for dropdown

        public BreedService(IBreedRepository breedRepository, ISpeciesRepository speciesRepository)
        {
            _breedRepository = breedRepository;
            _speciesRepository = speciesRepository;
        }

        public async Task<List<BreedWithSpeciesDto>> GetAllBreedsForAdminAsync()
        {
            var breeds = await _breedRepository.GetAllBreedsWithSpeciesAsync();
            return breeds.Select(b => new BreedWithSpeciesDto
            {
                Id = b.Id,
                BreedName = b.BreedName,
                SpeciesId = b.SpeciesId,
                SpeciesName = b.Species?.SpeciesName ?? "N/A", // Safely get SpeciesName
                IsDeleted = b.IsDeleted
            }).ToList();
        }

        public Task<Breed?> GetByIdAsync(string id)
        {
            return _breedRepository.GetByIdAsync(id);
        }

        public Task AddAsync(Breed breed)
        {
            return _breedRepository.AddAsync(breed);
        }

        public Task UpdateAsync(Breed breed)
        {
            return _breedRepository.UpdateAsync(breed);
        }

        public async Task SoftDeleteAsync(string id)
        {
            await _breedRepository.SoftDeleteAsync(id);
        }

        public async Task<List<Species>> GetAllSpeciesAsync()
        {
            // Use GetAllSpeciesWithBreedsAsync, but we only need the Species objects
            // The global query filter will ensure only non-deleted species are returned.
            var allSpecies = await _speciesRepository.GetAllSpeciesWithBreedsAsync();
            return allSpecies.Where(s => !s.IsDeleted).ToList(); // Ensure only non-deleted species are returned
        }
    }
}