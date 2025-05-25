// PetQuestV1.Data.Defines/IUserRepository.cs (or PetQuestV1.Contracts/Repositories/IUserRepository.cs)
using PetQuestV1.Data; // For ApplicationUser
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Data.Defines // Or your chosen namespace for repository interfaces
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(string userId);
        Task<ApplicationUser?> GetByUsernameAsync(string username);
        Task AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task DeleteAsync(string userId); // For hard delete
        Task SoftDeleteAsync(string userId);
        Task RestoreAsync(string userId);
        // Add any other core CRUD or specific query methods for ApplicationUser here
    }
}