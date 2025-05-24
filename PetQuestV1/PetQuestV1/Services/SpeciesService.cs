using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Services
{
    public class SpeciesService : ISpeciesService
    {
        // This is a simplified in-memory store for demonstration.
        // In a real application, you would interact with a database (e.g., Entity Framework Core).
        private static List<Species> _speciesData = new List<Species>
        {
            new Species { Id = Guid.NewGuid().ToString("N"), SpeciesName = "Dog", IsDeleted = false },
            new Species { Id = Guid.NewGuid().ToString("N"), SpeciesName = "Cat", IsDeleted = false },
            new Species { Id = Guid.NewGuid().ToString("N"), SpeciesName = "Bird", IsDeleted = false },
            new Species { Id = Guid.NewGuid().ToString("N"), SpeciesName = "Fish", IsDeleted = false }
        };

        public Task<IEnumerable<Species>> GetAllSpeciesAsync()
        {
            return Task.FromResult(_speciesData.Where(s => !s.IsDeleted).AsEnumerable());
        }

        public Task<Species> GetSpeciesByIdAsync(string id)
        {
            return Task.FromResult(_speciesData.FirstOrDefault(s => s.Id == id));
        }

        public Task AddSpeciesAsync(Species species)
        {
            if (string.IsNullOrEmpty(species.Id))
            {
                species.Id = Guid.NewGuid().ToString("N");
            }
            species.IsDeleted = false;
            _speciesData.Add(species);
            return Task.CompletedTask;
        }

        public Task UpdateSpeciesAsync(Species species)
        {
            var existingSpecies = _speciesData.FirstOrDefault(s => s.Id == species.Id);
            if (existingSpecies != null)
            {
                existingSpecies.SpeciesName = species.SpeciesName;
                // Update other properties as needed
            }
            return Task.CompletedTask;
        }

        public Task SoftDeleteSpeciesAsync(string id)
        {
            var speciesToSoftDelete = _speciesData.FirstOrDefault(s => s.Id == id);
            if (speciesToSoftDelete != null)
            {
                speciesToSoftDelete.IsDeleted = true;
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Species>> GetAllSpeciesIncludingDeletedAsync()
        {
            return Task.FromResult(_speciesData.AsEnumerable());
        }
    }
}