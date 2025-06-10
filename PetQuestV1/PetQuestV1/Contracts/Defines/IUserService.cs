using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; 
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PetQuestV1.Contracts.Defines
{
    public interface IUserService
    {
        Task<List<UserListItemDto>> GetAllUsersWithPetCountsAsync();
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<UserDetailDto?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);
        Task SoftDeleteUserAsync(string userId);
        Task RestoreUserAsync(string userId);
        Task HardDeleteUserAsync(string userId);
        Task UpdateUserAsync(UserFormDto userDto);
        Task<List<IdentityRole>> GetAllRolesAsync();
    }
}