using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Data.Defines;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;
using System;
using System.Linq; 

namespace PetQuestV1.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;
        private readonly IWebHostEnvironment _env;
        private const string ImagesFolderName = "images";
        private const string PetImagesSubfolderName = "pets";

        public PetService(IPetRepository petRepository, IWebHostEnvironment env)
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

        public Task<List<Pet>> GetPetsByOwnerIdAsync(string ownerId)
        {
            return _petRepository.GetPetsByOwnerIdAsync(ownerId);
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
                Advantage = petDto.Advantage, 
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
                petToUpdate.Advantage = petDto.Advantage; 
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

        public async Task<string?> UploadPetImageAsync(string petId, IBrowserFile imageFile)
        {
            var pet = await _petRepository.GetByIdAsync(petId);
            if (pet == null)
            {
                return null; 
            }

            var petImagesDirectoryPath = Path.Combine(_env.WebRootPath, ImagesFolderName, PetImagesSubfolderName);

            if (!Directory.Exists(petImagesDirectoryPath))
            {
                Directory.CreateDirectory(petImagesDirectoryPath);
            }

            var fileExtension = Path.GetExtension(imageFile.Name);
            var uniqueFileName = $"{Guid.NewGuid().ToString("N")}{fileExtension}";
            var fullFilePath = Path.Combine(petImagesDirectoryPath, uniqueFileName);

            const long maxAllowedSize = 5 * 1024 * 1024; // 5 MB

            try
            {
                await using FileStream fs = new(fullFilePath, FileMode.Create);
                await imageFile.OpenReadStream(maxAllowedSize).CopyToAsync(fs);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error saving file for pet {petId}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during file upload for pet {petId}: {ex.Message}");
                return null;
            }

            pet.ImagePath = $"/{ImagesFolderName}/{PetImagesSubfolderName}/{uniqueFileName}";

            await _petRepository.UpdateAsync(pet);

            return pet.ImagePath;
        }

        public async Task<bool> DeletePetImageAsync(string petId)
        {
            var pet = await _petRepository.GetByIdAsync(petId);
            if (pet == null || string.IsNullOrEmpty(pet.ImagePath))
            {
                return false;
            }

            var relativePath = pet.ImagePath.TrimStart('/');
            var fullFilePath = Path.Combine(_env.WebRootPath, relativePath);

            try
            {
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error deleting file for pet {petId} at {fullFilePath}: {ex.Message}");
                return false;
            }

            pet.ImagePath = null;
            await _petRepository.UpdateAsync(pet);
            return true;
        }

        public async Task<List<PetFormDto>> GetAllPetsFormDtoAsync()
        {
            var pets = await _petRepository.GetAllPetsWithDetailsAsync();
            return pets.Select(p => new PetFormDto
            {
                Id = p.Id,
                PetName = p.PetName,
                SpeciesId = p.SpeciesId,
                BreedId = p.BreedId,
                OwnerId = p.OwnerId,
                Age = p.Age,
                Advantage = p.Advantage,
                ImagePath = p.ImagePath
            }).ToList();
        }
        public async Task<PetFormDto?> GetPetFormDtoByIdAsync(string id)
        {
            var pet = await _petRepository.GetPetWithDetailsByIdAsync(id); 
            if (pet == null)
            {
                return null;
            }

            return new PetFormDto
            {
                Id = pet.Id,
                PetName = pet.PetName,
                SpeciesId = pet.SpeciesId,
                BreedId = pet.BreedId,
                OwnerId = pet.OwnerId,
                Age = pet.Age,
                Advantage = pet.Advantage,
                ImagePath = pet.ImagePath
            };
        }

        public async Task<List<PetViewerDto>> GetAllPetsForAnalystAsync()
        {
            var pets = await _petRepository.GetAllPetsWithDetailsAsync(); 
            return pets.Select(p => new PetViewerDto
            {
                Id = p.Id,
                PetName = p.PetName,
                SpeciesName = p.Species?.SpeciesName ?? "N/A",
                BreedName = p.Breed?.BreedName ?? "N/A",
                OwnerName = p.Owner?.UserName ?? "N/A",
                Age = p.Age,
                Advantage = p.Advantage,
                ImagePath = p.ImagePath
            }).ToList();
        }
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