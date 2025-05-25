// PetQuestV1/Contracts/Defines/ISpeciesService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models; // For Species and SpeciesWithBreedCountDto

namespace PetQuestV1.Contracts.Defines
{
    public interface ISpeciesService
    {
        // Changed to return the DTO with breed count
        Task<List<SpeciesWithBreedCountDto>> GetAllSpeciesForAdminAsync(); // <--- NEW/MODIFIED METHOD
        Task<Species?> GetByIdAsync(string id); // Keep for fetching the full Species object for editing
        Task AddAsync(Species species);
        Task UpdateAsync(Species species);
        Task SoftDeleteAsync(string id);
    }
}