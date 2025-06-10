using PetQuestV1.Data; 
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
        Task DeleteAsync(string userId);
        Task SoftDeleteAsync(string userId);
        Task RestoreAsync(string userId);
    }
}