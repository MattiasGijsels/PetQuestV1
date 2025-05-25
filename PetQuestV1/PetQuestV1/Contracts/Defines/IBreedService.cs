// PetQuestV1/Contracts/Defines/IBreedService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Contracts.Defines
{
    public interface IBreedService
    {
        Task<List<BreedWithSpeciesDto>> GetAllBreedsForAdminAsync();
        Task<Breed?> GetByIdAsync(string id);
        Task AddAsync(Breed breed);
        Task UpdateAsync(Breed breed);
        Task SoftDeleteAsync(string id);
        Task<List<Species>> GetAllSpeciesAsync(); // Needed for dropdown in Breed Admin Panel
    }
}