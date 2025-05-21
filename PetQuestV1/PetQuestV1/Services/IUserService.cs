// Contracts/IUserService.cs
using PetQuestV1.Data; // For ApplicationUser
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetQuestV1.Contracts
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username); // Useful for login/finding
        Task SoftDeleteUserAsync(string userId);
        Task RestoreUserAsync(string userId); // To un-delete a user
        Task HardDeleteUserAsync(string userId); // For permanent deletion (use with caution)
        Task UpdateUserAsync(ApplicationUser user); // For updating user profile data
        // You might add more methods here, e.g., AddUser (if not using built-in registration),
        // AddUserToRole, RemoveUserFromRole, etc., depending on your needs.
    }
}