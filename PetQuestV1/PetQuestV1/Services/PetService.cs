using PetQuestV1.Contracts;
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.Models;
using PetQuestV1.Contracts.DTOs.Pets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
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
                Age = petDto.Age, // Direct assignment, types match
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
                petToUpdate.Age = petDto.Age; // Direct assignment, types match
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