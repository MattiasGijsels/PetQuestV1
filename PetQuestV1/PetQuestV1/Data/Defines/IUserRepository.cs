using PetQuestV1.Data; // For ApplicationUser
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Data.Defines 
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
    }
}