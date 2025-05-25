using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; // For ApplicationUser
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For IdentityRole

namespace PetQuestV1.Contracts.Defines
{
    public interface IUserService
    {
        Task<List<UserListItemDto>> GetAllUsersWithPetCountsAsync();
        Task<List<ApplicationUser>> GetAllUsersAsync(); // Keep if still needed elsewhere, might need it later
        Task<UserDetailDto?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);
        Task SoftDeleteUserAsync(string userId);
        Task RestoreUserAsync(string userId); // Might use this if I have time left
        Task HardDeleteUserAsync(string userId);
        Task UpdateUserAsync(UserFormDto userDto);
        Task<List<IdentityRole>> GetAllRolesAsync();
    }
}