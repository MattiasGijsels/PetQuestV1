// PetQuestV1.Services/UserService.cs
using PetQuestV1.Contracts.Defines;
using PetQuestV1.Contracts.DTOs;
using PetQuestV1.Data; // Assuming this is where ApplicationUser and ApplicationDbContext reside
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Needed for ToListAsync, FindAsync, SaveChangesAsync etc.

namespace PetQuestV1.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext; // Inject your DbContext

        public UserService(ApplicationDbContext dbContext) // Constructor for DI
        {
            _dbContext = dbContext;
        }

        // Change return type to the DTO
        public async Task<List<UserListItemDto>> GetAllUsersWithPetCountsAsync() // Renamed for clarity
        {
            // Fetch users and their pet counts from the database
            // This example assumes a 'Pets' navigation property on ApplicationUser
            // You might need to adjust this query based on your actual data model
            return await _dbContext.Users
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty, // Handle potential null UserName
                    Email = u.Email ?? string.Empty,       // Handle potential null Email
                    PetCount = u.Pets.Count(), // Assuming 'Pets' is a navigation property or similar relation
                    IsDeleted = u.IsDeleted // Assuming ApplicationUser has an IsDeleted property
                })
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        // --- MODIFIED: Return UserDetailDto ---
        public async Task<UserDetailDto?> GetUserByIdAsync(string userId)
        {
            // Find the ApplicationUser entity by ID
            var user = await _dbContext.Users
                                       .Where(u => u.Id == userId)
                                       // .Include(u => u.SomeRelatedData) // Include any related data needed for UserDetailDto
                                       .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            // Map the ApplicationUser entity to the UserDetailDto
            return new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                // Ensure PetCount is correctly populated, if it's not a direct property of ApplicationUser
                // it might need a separate lookup or a calculated property.
                // For simplicity, we'll assume it's directly available or can be calculated:
                PetCount = user.Pets?.Count() ?? 0, // Assuming a 'Pets' navigation property
                IsDeleted = user.IsDeleted, // Assuming ApplicationUser has an IsDeleted property
                // Map other properties from ApplicationUser to UserDetailDto
                // Example: OtherProperty = user.OtherProperty
            };
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username) // Useful for login/finding
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task SoftDeleteUserAsync(string userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsDeleted = true; // Assuming ApplicationUser has an IsDeleted property
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RestoreUserAsync(string userId) // To un-delete a user *maybe for future use*
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsDeleted = false; // Assuming ApplicationUser has an IsDeleted property
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task HardDeleteUserAsync(string userId) // For permanent deletion (use with caution)*maybe for future use*
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        // --- MODIFIED: Accept UserFormDto for updating user profile data ---
        public async Task UpdateUserAsync(UserFormDto userDto)
        {
            var user = await _dbContext.Users.FindAsync(userDto.Id); // Find the entity by ID
            if (user != null)
            {
                // Update properties of the ApplicationUser entity from the UserFormDto
                user.UserName = userDto.UserName;
                user.Email = userDto.Email;
                user.IsDeleted = userDto.IsDeleted;

                // Important: PetCount is usually a calculated field (number of pets linked to a user).
                // It's generally not directly updated via a user profile edit form.
                // If you *do* intend to allow direct editing of PetCount here,
                // you would need to handle the implications (e.g., creating/deleting pets to match the count).
                // For most cases, you would *not* update PetCount directly from UserFormDto here.
                // user.PetCount = userDto.PetCount; // <-- Comment this out or remove if PetCount is derived

                // You might need to update security-related fields carefully if the form allows it.
                // e.g., if changing email needs re-confirmation, handle that separately.
                // For security properties, use UserManager methods if applicable.

                _dbContext.Users.Update(user); // Mark the entity as modified
                await _dbContext.SaveChangesAsync(); // Save changes to the database
            }
        }

        // You might add more methods here, e.g., AddUser (if not using built-in registration),
        // AddUserToRole, RemoveUserFromRole, etc., depending on your needs.
        /*
        public async Task AddUserAsync(UserFormDto userDto)
        {
            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(), // Generate a new ID
                UserName = userDto.UserName,
                Email = userDto.Email,
                // Other properties
                IsDeleted = false // New users are not deleted by default
            };
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
        }
        */
    }
}