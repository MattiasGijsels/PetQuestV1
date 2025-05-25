// PetQuestV1/Data/Defines/ISpeciesRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Data.Defines
{
    public interface ISpeciesRepository
    {
        Task<List<Species>> GetAllSpeciesWithBreedsAsync(); 
        Task<Species?> GetByIdAsync(string id);
        Task AddAsync(Species species);
        Task UpdateAsync(Species species);
        Task SoftDeleteAsync(string id);
        Task HardDeleteAsync(string id);
    }
}