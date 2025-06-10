using PetQuestV1.Data.Defines;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.DTOs;
using System.Collections.Generic;
using System.Linq; 
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

        public async Task<List<SpeciesWithBreedCountDto>> GetAllSpeciesForAdminAsync() 
        {
            var speciesList = await _speciesRepository.GetAllSpeciesWithBreedsAsync();
            return speciesList.Select(s => new SpeciesWithBreedCountDto
            {
                Id = s.Id,
                SpeciesName = s.SpeciesName,
                IsDeleted = s.IsDeleted,
                BreedCount = s.Breeds?.Count ?? 0 
            }).ToList();
        }

        public Task<Species?> GetByIdAsync(string id)
        {
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