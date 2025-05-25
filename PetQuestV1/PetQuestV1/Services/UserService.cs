// PetQuestV1.Services/UserService.cs
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; // For ApplicationUser
using PetQuestV1.Data.Defines; // For IUserRepository
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For UserManager and RoleManager
using Microsoft.EntityFrameworkCore; // For ToListAsync etc.

namespace PetQuestV1.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; // Keep for non-Identity specific user queries (e.g., GetAllAsync with Pets)
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(IUserRepository userRepository,
                           UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserListItemDto>> GetAllUsersWithPetCountsAsync()
        {
            // This method might still use _userRepository if it's optimized for specific includes
            // that UserManager.Users does not automatically provide (like Pets).
            // UserManager.Users queryable will likely not eager load Pets without explicit .Include().
            var users = await _userRepository.GetAllAsync(); // This includes .Include(u => u.Pets)

            var userListItems = new List<UserListItemDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault() ?? "No Role";

                userListItems.Add(new UserListItemDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PetCount = user.Pets?.Count() ?? 0,
                    IsDeleted = user.IsDeleted,
                    RoleName = roleName
                });
            }
            return userListItems;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(string userId)
        {
            // Use UserManager to get the user when doing Identity operations
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Manually load Pets if needed, as FindByIdAsync doesn't include them by default.
            // This could be done by using _userRepository.GetByIdAsync(userId) instead of _userManager.FindByIdAsync(userId)
            // if _userRepository.GetByIdAsync already includes Pets and you need it for PetCount.
            // For now, let's assume PetCount in UserDetailDto isn't directly updated via form,
            // or we need to ensure Pets are loaded. The easiest way to get PetCount is still from the repository.
            // Let's refine this to use the repository's get method to ensure Pets are loaded.
            var userWithPets = await _userRepository.GetByIdAsync(userId);
            if (userWithPets == null) // This check is redundant if user is already found above
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault();
            var currentRoleEntity = currentRole != null ? await _roleManager.FindByNameAsync(currentRole) : null;

            return new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PetCount = userWithPets.Pets?.Count() ?? 0, // Use PetCount from userWithPets
                IsDeleted = user.IsDeleted,
                SelectedRoleId = currentRoleEntity?.Id ?? string.Empty
            };
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username); // Keep this for specific username queries
        }

        public async Task SoftDeleteUserAsync(string userId)
        {
            // This is a custom property, so UserRepository is appropriate
            await _userRepository.SoftDeleteAsync(userId);
        }

        public async Task RestoreUserAsync(string userId)
        {
            // This is a custom property, so UserRepository is appropriate
            await _userRepository.RestoreAsync(userId);
        }

        public async Task HardDeleteUserAsync(string userId)
        {
            // Hard delete user via UserManager. It handles cascading deletes for Identity tables.
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                // You might want to check result.Succeeded and handle errors
            }
            // If _userRepository.DeleteAsync also handles other associated data (like Pets),
            // you might need to decide which method is the "master" for hard deletion,
            // or call both if they handle different aspects. For now, rely on UserManager
            // for Identity-related deletion.
        }

        public async Task UpdateUserAsync(UserFormDto userDto)
        {
            // IMPORTANT: Get the user using UserManager.FindByIdAsync to ensure it's tracked
            // by the same context that UserManager uses.
            var user = await _userManager.FindByIdAsync(userDto.Id!);

            if (user != null)
            {
                // Update basic user properties directly on the tracked entity
                user.UserName = userDto.UserName;
                user.Email = userDto.Email;
                user.IsDeleted = userDto.IsDeleted;

                // Persist changes to basic user properties using UserManager
                // This will also save changes to UserName, Email, IsDeleted, etc.
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    // Handle errors, e.g., throw exception or return IdentityResult
                    throw new InvalidOperationException($"Failed to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }

                // --- Handle Role Update ---
                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRoleName = currentRoles.FirstOrDefault();

                var newRoleEntity = await _roleManager.FindByIdAsync(userDto.SelectedRoleId);
                var newRoleName = newRoleEntity?.Name;

                if (currentRoleName != newRoleName)
                {
                    // Remove existing roles
                    if (!string.IsNullOrEmpty(currentRoleName))
                    {
                        var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRoleName);
                        if (!removeResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Failed to remove role '{currentRoleName}': {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                        }
                    }

                    // Add the new role if one is selected
                    if (!string.IsNullOrEmpty(newRoleName))
                    {
                        var addResult = await _userManager.AddToRoleAsync(user, newRoleName);
                        if (!addResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Failed to add role '{newRoleName}': {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                        }
                    }
                }
            }
            else
            {
                throw new KeyNotFoundException($"User with ID {userDto.Id} not found.");
            }
        }

        public async Task<List<IdentityRole>> GetAllRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }
    }
}