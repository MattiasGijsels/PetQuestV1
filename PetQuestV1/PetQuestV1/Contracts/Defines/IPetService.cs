using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.DTOs.Pets; // Add this using statement

namespace PetQuestV1.Contracts.Defines
{
    public interface IPetService
    {
        Task<List<Pet>> GetAllAsync();
        Task<Pet?> GetByIdAsync(string id);

        // Updated signatures to use PetFormDto
        Task AddPetAsync(PetFormDto petDto); // Renamed to avoid confusion with the Model
        Task UpdatePetAsync(PetFormDto petDto); // Renamed to avoid confusion with the Model

        Task DeleteAsync(string id);
        Task SoftDeleteAsync(string id);

        Task<Species?> GetSpeciesByNameAsync(string name);
        Task<List<Species>> GetAllSpeciesAsync();

        Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId);
        Task<List<Breed>> GetAllBreedsAsync();
        Task<Breed?> GetBreedByIdAsync(string id);
    }
}