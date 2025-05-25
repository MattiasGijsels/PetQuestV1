using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.DTOs.Pets;

namespace PetQuestV1.Contracts.Defines
{
    public interface IPetService
    {
        Task<List<Pet>> GetAllAsync();
        Task<Pet?> GetByIdAsync(string id);

        Task AddPetAsync(PetFormDto petDto);
        Task UpdatePetAsync(PetFormDto petDto);
        Task DeleteAsync(string id);
        Task SoftDeleteAsync(string id);

        Task<Species?> GetSpeciesByNameAsync(string name);
        Task<List<Species>> GetAllSpeciesAsync();
        Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId);
        Task<List<Breed>> GetAllBreedsAsync();
        Task<Breed?> GetBreedByIdAsync(string id);
    }
}