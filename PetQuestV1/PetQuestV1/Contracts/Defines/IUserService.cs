// PetQuestV1.Contracts.Defines/IUserService.cs
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
        Task<List<ApplicationUser>> GetAllUsersAsync(); // Keep if still needed elsewhere

        Task<UserDetailDto?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);

        Task SoftDeleteUserAsync(string userId);
        Task RestoreUserAsync(string userId);
        Task HardDeleteUserAsync(string userId);

        Task UpdateUserAsync(UserFormDto userDto);

        // New method to get all roles for the dropdown
        Task<List<IdentityRole>> GetAllRolesAsync();
    }
}