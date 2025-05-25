// PetQuestV1.Services/UserService.cs
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; // For ApplicationUser
using PetQuestV1.Data.Defines; // For IUserRepository
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Removed: using Microsoft.EntityFrameworkCore; // Not needed here anymore

namespace PetQuestV1.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository; // Inject the repository interface

        public UserService(IUserRepository userRepository) // Constructor for DI
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserListItemDto>> GetAllUsersWithPetCountsAsync()
        {
            // Now use the repository to get the data
            var users = await _userRepository.GetAllAsync();

            return users.Select(u => new UserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                PetCount = u.Pets?.Count() ?? 0, // Utilize the loaded Pets navigation property
                IsDeleted = u.IsDeleted
            })
            .ToList();
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            // Direct pass-through to the repository.
            // Consider if this method is truly needed on the service layer, or if DTOs are always preferred.
            return await _userRepository.GetAllAsync();
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PetCount = user.Pets?.Count() ?? 0,
                IsDeleted = user.IsDeleted
            };
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task SoftDeleteUserAsync(string userId)
        {
            // The service layer holds the business logic: "soft delete this user".
            // The repository handles the data access implementation of that logic.
            await _userRepository.SoftDeleteAsync(userId);
        }

        public async Task RestoreUserAsync(string userId)
        {
            await _userRepository.RestoreAsync(userId);
        }

        public async Task HardDeleteUserAsync(string userId)
        {
            await _userRepository.DeleteAsync(userId);
        }

        public async Task UpdateUserAsync(UserFormDto userDto)
        {
            // Fetch the existing user from the repository
            var user = await _userRepository.GetByIdAsync(userDto.Id!); // Assuming Id is not null for update

            if (user != null)
            {
                // Update properties of the entity from the DTO
                user.UserName = userDto.UserName;
                user.Email = userDto.Email;
                user.IsDeleted = userDto.IsDeleted;

                // PetCount is typically derived, not directly updated here.
                // If you uncommented this in your original code, make sure you understand the implications.
                // user.PetCount = userDto.PetCount;

                // Pass the updated entity to the repository for persistence
                await _userRepository.UpdateAsync(user);
            }
            // You might want to throw an exception or return a status if user is not found
        }

        // Example of adding a user, if you implement a create endpoint
        /*
        public async Task CreateUserAsync(UserFormDto userDto)
        {
            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(), // Generate a new ID
                UserName = userDto.UserName,
                Email = userDto.Email,
                EmailConfirmed = true, // Or false, depending on your registration flow
                IsDeleted = false,
                // Add any other required properties for a new user
            };

            await _userRepository.AddAsync(newUser);
        }
        */
    }
}