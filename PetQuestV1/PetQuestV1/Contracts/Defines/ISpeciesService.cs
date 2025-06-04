using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models; 
using PetQuestV1.Contracts.DTOs;

namespace PetQuestV1.Contracts.Defines
{
    public interface ISpeciesService
    {
        Task<List<SpeciesWithBreedCountDto>> GetAllSpeciesForAdminAsync();
        Task<Species?> GetByIdAsync(string id); //for fetching the full Species object for editing
        Task AddAsync(Species species);
        Task UpdateAsync(Species species);
        Task SoftDeleteAsync(string id);
    }
}