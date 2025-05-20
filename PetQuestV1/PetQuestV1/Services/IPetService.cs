using System.Collections.Generic;
using System.Threading.Tasks;
using PetQuestV1.Contracts.Models;

namespace PetQuestV1.Contracts
{
    public interface IPetService
    {
        Task<List<Pet>> GetAllAsync();
        Task<Pet?> GetByIdAsync(string id);
        Task AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(string id);
        Task<Species?> GetSpeciesByNameAsync(string name);
    }
}