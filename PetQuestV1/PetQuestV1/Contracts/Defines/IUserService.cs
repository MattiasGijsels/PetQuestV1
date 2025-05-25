// PetQuestV1.Contracts.Defines/IUserService.cs
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; // For ApplicationUser (if still needed for GetAllUsersAsync)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Contracts.Defines
{
    public interface IUserService
    {
        Task<List<UserListItemDto>> GetAllUsersWithPetCountsAsync();
        // Keep this if other parts of your app still need the raw ApplicationUser list
        // If not needed by anything *outside* the service layer, consider removing.
        Task<List<ApplicationUser>> GetAllUsersAsync(); // This might be removed later

        Task<UserDetailDto?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);

        Task SoftDeleteUserAsync(string userId);
        Task RestoreUserAsync(string userId);
        Task HardDeleteUserAsync(string userId);

        Task UpdateUserAsync(UserFormDto userDto);
    }
}