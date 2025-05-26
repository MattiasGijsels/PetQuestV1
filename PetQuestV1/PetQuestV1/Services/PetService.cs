// Services/PetService.cs
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.DTOs.Pets;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Data.Defines;
using Microsoft.AspNetCore.Hosting; // Required for IWebHostEnvironment
using Microsoft.AspNetCore.Components.Forms; // Required for IBrowserFile
using System.IO; // Required for Path and Directory operations
using System; // Required for Guid

namespace PetQuestV1.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;
        private readonly IWebHostEnvironment _env; // Inject IWebHostEnvironment
        private const string ImagesFolderName = "images"; // Consistent folder name for images in wwwroot
        private const string PetImagesSubfolderName = "pets"; // Subfolder specific to pet images

        public PetService(IPetRepository petRepository, IWebHostEnvironment env) // Add IWebHostEnvironment to constructor
        {
            _petRepository = petRepository;
            _env = env;
        }

        public Task<List<Pet>> GetAllAsync()
        {
            return _petRepository.GetAllAsync();
        }

        public Task<Pet?> GetByIdAsync(string id)
        {
            return _petRepository.GetByIdAsync(id);
        }

        public async Task AddPetAsync(PetFormDto petDto)
        {
            var pet = new Pet
            {
                PetName = petDto.PetName,
                SpeciesId = petDto.SpeciesId,
                BreedId = petDto.BreedId,
                OwnerId = petDto.OwnerId,
                Age = petDto.Age,
                IsDeleted = false
            };
            await _petRepository.AddAsync(pet);
        }

        public async Task UpdatePetAsync(PetFormDto petDto)
        {
            var petToUpdate = await _petRepository.GetByIdAsync(petDto.Id!);

            if (petToUpdate != null)
            {
                petToUpdate.PetName = petDto.PetName;
                petToUpdate.SpeciesId = petDto.SpeciesId;
                petToUpdate.BreedId = petDto.BreedId;
                petToUpdate.OwnerId = petDto.OwnerId;
                petToUpdate.Age = petDto.Age;
                // Important: The ImagePath is updated via UploadPetImageAsync, not directly from PetFormDto
                // Do NOT touch petToUpdate.IsDeleted here
                await _petRepository.UpdateAsync(petToUpdate);
            }
        }

        public Task DeleteAsync(string id)
        {
            return _petRepository.DeleteAsync(id);
        }

        public async Task SoftDeleteAsync(string id)
        {
            var pet = await _petRepository.GetByIdAsync(id);
            if (pet != null)
            {
                pet.IsDeleted = true;
                await _petRepository.UpdateAsync(pet);
            }
        }

        // --- New Methods for Image Handling ---

        public async Task<string?> UploadPetImageAsync(string petId, IBrowserFile imageFile)
        {
            var pet = await _petRepository.GetByIdAsync(petId);
            if (pet == null)
            {
                return null; // Pet not found
            }

            // Construct the full server path for the pet images directory
            var petImagesDirectoryPath = Path.Combine(_env.WebRootPath, ImagesFolderName, PetImagesSubfolderName);

            // Ensure the directory exists; create it if it doesn't
            if (!Directory.Exists(petImagesDirectoryPath))
            {
                Directory.CreateDirectory(petImagesDirectoryPath);
            }

            // Generate a unique file name to avoid collisions
            var fileExtension = Path.GetExtension(imageFile.Name);
            var uniqueFileName = $"{Guid.NewGuid().ToString("N")}{fileExtension}";
            var fullFilePath = Path.Combine(petImagesDirectoryPath, uniqueFileName);

            // Set a reasonable maximum file size (e.g., 5 MB)
            const long maxAllowedSize = 5 * 1024 * 1024; // 5 MB

            try
            {
                // Save the file to the server's file system
                await using FileStream fs = new(fullFilePath, FileMode.Create);
                await imageFile.OpenReadStream(maxAllowedSize).CopyToAsync(fs);
            }
            catch (IOException ex)
            {
                // Log the exception for debugging, especially for local setup issues (e.g., permissions)
                Console.WriteLine($"Error saving file for pet {petId}: {ex.Message}");
                // In a production app, you'd use a proper logger (e.g., ILogger)
                return null; // Indicate failure
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during file upload for pet {petId}: {ex.Message}");
                return null;
            }

            // Store the public URL/path that the web browser can access
            pet.ImagePath = $"/{ImagesFolderName}/{PetImagesSubfolderName}/{uniqueFileName}";

            // Update the pet's ImagePath in the database
            await _petRepository.UpdateAsync(pet);

            return pet.ImagePath; // Return the path for immediate UI display
        }

        public async Task<bool> DeletePetImageAsync(string petId)
        {
            var pet = await _petRepository.GetByIdAsync(petId);
            if (pet == null || string.IsNullOrEmpty(pet.ImagePath))
            {
                return false; // Pet not found or no image to delete
            }

            // Convert the public URL back to the full server path
            var relativePath = pet.ImagePath.TrimStart('/'); // Remove leading '/'
            var fullFilePath = Path.Combine(_env.WebRootPath, relativePath);

            try
            {
                // Delete the file from the server's file system if it exists
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error deleting file for pet {petId} at {fullFilePath}: {ex.Message}");
                return false; // Indicate deletion failed
            }

            // Clear the ImagePath from the database
            pet.ImagePath = null;
            await _petRepository.UpdateAsync(pet);
            return true; // Indicate successful deletion
        }

        // --- Existing Methods (unchanged from your provided code) ---
        public Task<Species?> GetSpeciesByNameAsync(string name)
        {
            return _petRepository.GetSpeciesByNameAsync(name);
        }

        public Task<List<Species>> GetAllSpeciesAsync()
        {
            return _petRepository.GetAllSpeciesAsync();
        }

        public Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId)
        {
            return _petRepository.GetBreedsBySpeciesIdAsync(speciesId);
        }

        public Task<List<Breed>> GetAllBreedsAsync()
        {
            return _petRepository.GetAllBreedsAsync();
        }

        public Task<Breed?> GetBreedByIdAsync(string id)
        {
            return _petRepository.GetBreedByIdAsync(id);
        }
    }
}