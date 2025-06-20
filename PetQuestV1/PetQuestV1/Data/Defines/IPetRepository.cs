﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Data.Defines
{
    public interface IPetRepository
    {
        Task<List<Pet>> GetAllAsync();
        Task<Pet?> GetByIdAsync(string id);
        Task<List<Pet>> GetPetsByOwnerIdAsync(string ownerId);
        Task AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(string id);
        Task<Species?> GetSpeciesByNameAsync(string name); 
        Task<List<Species>> GetAllSpeciesAsync();
        Task<List<Breed>> GetBreedsBySpeciesIdAsync(string speciesId);
        Task<List<Breed>> GetAllBreedsAsync(); 
        Task<Breed?> GetBreedByIdAsync(string id); 
        Task<List<Pet>> GetAllPetsWithDetailsAsync();
        Task<Pet?> GetPetWithDetailsByIdAsync(string id);
    }
}