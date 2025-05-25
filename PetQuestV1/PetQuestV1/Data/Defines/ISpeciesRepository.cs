// PetQuestV1/Data/Defines/ISpeciesRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Data.Defines
{
    public interface ISpeciesRepository
    {
        // Change the return type to the full Species model, but include breeds for counting.
        // We need the ICollection<Breed> to be populated for the service to count.
        Task<List<Species>> GetAllSpeciesWithBreedsAsync(); // <--- NEW METHOD SIGNATURE
        Task<Species?> GetByIdAsync(string id);
        Task AddAsync(Species species);
        Task UpdateAsync(Species species);
        Task SoftDeleteAsync(string id);
        Task HardDeleteAsync(string id);
    }
}