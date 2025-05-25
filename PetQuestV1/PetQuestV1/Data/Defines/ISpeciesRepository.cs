// PetQuestV1/Data/Defines/ISpeciesRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models; // For the Species model

namespace PetQuestV1.Data.Defines
{
    public interface ISpeciesRepository
    {
        Task<List<Species>> GetAllAsync();
        Task<Species?> GetByIdAsync(string id);
        Task AddAsync(Species species);
        Task UpdateAsync(Species species);
        Task SoftDeleteAsync(string id); // For soft deletion
        Task HardDeleteAsync(string id); // Optional: for permanent deletion if ever needed
    }
}