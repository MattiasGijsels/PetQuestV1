// Contracts/Defines/IPetService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.DTOs.Pets;
using Microsoft.AspNetCore.Components.Forms; // Required for IBrowserFile

namespace PetQuestV1.Contracts.Defines
{
    public interface IPetService
    {
        Task<List<Pet>> GetAllAsync();
        Task<Pet?> GetByIdAsync(string id);

        Task AddPetAsync(PetFormDto petDto); // Consider if this should return the Id or the Pet object
        Task UpdatePetAsync(PetFormDto petDto);
        Task DeleteAsync(string id);
        Task SoftDeleteAsync(string id);

        Task<List<Pet>> GetPetsByOwnerIdAsync(string ownerId); // NEW METHOD
        Task<Species?> GetSpeciesByNameAsync(string name);
        Task<List<Species>> GetAllSpeciesAsync();
        Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId);
        Task<List<Breed>> GetAllBreedsAsync();
        Task<Breed?> GetBreedByIdAsync(string id);

        // Handles uploading an image file for a given pet ID. Returns the public URL/path of the saved image.
        Task<string?> UploadPetImageAsync(string petId, IBrowserFile imageFile);

        // Handles deleting a pet's image. Returns true if successful.
        Task<bool> DeletePetImageAsync(string petId);
    }
}