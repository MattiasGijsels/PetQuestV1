// Contracts/Defines/IUserService.cs
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; // For ApplicationUser
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Contracts.Defines
{
    public interface IUserService
    {
        // Change return type to the DTO for list view
        Task<List<UserListItemDto>> GetAllUsersWithPetCountsAsync(); // Renamed for clarity

        // Keep this if other parts of your app still need the raw ApplicationUser list
        Task<List<ApplicationUser>> GetAllUsersAsync();

        // --- MODIFIED: Changed return type to UserDetailDto ---
        Task<UserDetailDto?> GetUserByIdAsync(string userId);

        Task<ApplicationUser?> GetUserByUsernameAsync(string username); // Useful for login/finding

        Task SoftDeleteUserAsync(string userId);
        Task RestoreUserAsync(string userId); // To un-delete a user *maybe for future use*
        Task HardDeleteUserAsync(string userId); // For permanent deletion (use with caution)*maybe for future use*

        // --- MODIFIED: Changed parameter type to UserFormDto ---
        Task UpdateUserAsync(UserFormDto userDto); // For updating user profile data

        // You might add more methods here, e.g., AddUser (if not using built-in registration),
        // AddUserToRole, RemoveUserFromRole, etc., depending on your needs.
        // If you plan to add new users via a form:
        // Task CreateUserAsync(UserFormDto userDto);
    }
}