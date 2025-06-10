using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Data.Defines
{
    public interface IBreedRepository
    {
        Task<List<Breed>> GetAllBreedsWithSpeciesAsync();
        Task<Breed?> GetByIdAsync(string id);
        Task AddAsync(Breed breed);
        Task UpdateAsync(Breed breed);
        Task SoftDeleteAsync(string id);
        Task HardDeleteAsync(string id);
    }
}