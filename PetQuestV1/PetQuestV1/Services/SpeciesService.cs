// PetQuestV1/Services/SpeciesService.cs
using PetQuestV1.Data.Defines;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using System.Collections.Generic;
using System.Linq; // Add this for LINQ methods like Select
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

        // Implementation of the new DTO-returning method
        public async Task<List<SpeciesWithBreedCountDto>> GetAllSpeciesForAdminAsync() // <--- MODIFIED METHOD
        {
            // Use the repository method that includes breeds
            var speciesList = await _speciesRepository.GetAllSpeciesWithBreedsAsync();

            // Map to DTO and calculate BreedCount
            // Since Breed also has a global query filter for IsDeleted,
            // s.Breeds will only contain non-deleted breeds.
            return speciesList.Select(s => new SpeciesWithBreedCountDto
            {
                Id = s.Id,
                SpeciesName = s.SpeciesName,
                IsDeleted = s.IsDeleted,
                BreedCount = s.Breeds?.Count ?? 0 // Safely count the included breeds
            }).ToList();
        }

        public Task<Species?> GetByIdAsync(string id)
        {
            // This still returns the full Species object, which is needed for the edit form.
            // The repository's GetByIdAsync now also includes Breeds, which is fine but not strictly
            // used by the current edit form logic for this property.
            return _speciesRepository.GetByIdAsync(id);
        }

        public Task AddAsync(Species species)
        {
            return _speciesRepository.AddAsync(species);
        }

        public Task UpdateAsync(Species species)
        {
            return _speciesRepository.UpdateAsync(species);
        }

        public async Task SoftDeleteAsync(string id)
        {
            await _speciesRepository.SoftDeleteAsync(id);
        }
    }
}