using PetQuestV1.Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Contracts.Services
{
    public interface ISpeciesService
    {
        Task<IEnumerable<Species>> GetAllSpeciesAsync();
        Task<Species> GetSpeciesByIdAsync(string id);
        Task AddSpeciesAsync(Species species);
        Task UpdateSpeciesAsync(Species species);
        Task SoftDeleteSpeciesAsync(string id);
        // You might want to add a method to get all species including soft-deleted ones for admin purposes
        Task<IEnumerable<Species>> GetAllSpeciesIncludingDeletedAsync();
    }
}