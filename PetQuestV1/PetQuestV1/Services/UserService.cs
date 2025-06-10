using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; 
using PetQuestV1.Data.Defines; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;

namespace PetQuestV1.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; 
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
            var users = await _userRepository.GetAllAsync(); 

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
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            var userWithPets = await _userRepository.GetByIdAsync(userId);
            if (userWithPets == null) 
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
                PetCount = userWithPets.Pets?.Count() ?? 0, 
                IsDeleted = user.IsDeleted,
                SelectedRoleId = currentRoleEntity?.Id ?? string.Empty
            };
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username); 
        }

        public async Task SoftDeleteUserAsync(string userId)
        {
            await _userRepository.SoftDeleteAsync(userId);
        }

        public async Task RestoreUserAsync(string userId)
        {
            await _userRepository.RestoreAsync(userId);
        }

        public async Task HardDeleteUserAsync(string userId)
        {
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
               
            }
            //For hard delete in the future
        }

        public async Task UpdateUserAsync(UserFormDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id!);

            if (user != null)
            {
               
                user.UserName = userDto.UserName;
                user.Email = userDto.Email;
                user.IsDeleted = userDto.IsDeleted;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                   
                    throw new InvalidOperationException($"Failed to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRoleName = currentRoles.FirstOrDefault();

                var newRoleEntity = await _roleManager.FindByIdAsync(userDto.SelectedRoleId);
                var newRoleName = newRoleEntity?.Name;

                if (currentRoleName != newRoleName)
                {
                    if (!string.IsNullOrEmpty(currentRoleName))
                    {
                        var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRoleName);
                        if (!removeResult.Succeeded)
                        {
                            throw new InvalidOperationException($"Failed to remove role '{currentRoleName}': {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                        }
                    }

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