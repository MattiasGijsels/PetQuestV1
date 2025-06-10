using Microsoft.AspNetCore.Components.Forms;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        Task<List<Pet>> GetPetsByOwnerIdAsync(string ownerId);
        Task<Species?> GetSpeciesByNameAsync(string name);
        Task<List<Species>> GetAllSpeciesAsync();
        Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId);
        Task<List<Breed>> GetAllBreedsAsync();
        Task<Breed?> GetBreedByIdAsync(string id);
        Task<string?> UploadPetImageAsync(string petId, IBrowserFile imageFile);
        Task<bool> DeletePetImageAsync(string petId);
        Task<List<PetFormDto>> GetAllPetsFormDtoAsync();
        Task<PetFormDto?> GetPetFormDtoByIdAsync(string id);
        Task<List<PetViewerDto>> GetAllPetsForAnalystAsync();

    }
}