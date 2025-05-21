// Services/UserService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // For ToListAsync() and IgnoreQueryFilters()
using PetQuestV1.Contracts;
using PetQuestV1.Data; // For ApplicationUser and ApplicationDbContext
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // Inject DbContext for specific queries if UserManager doesn't suffice

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context; // Inject DbContext here if you need direct DB access beyond UserManager's capabilities
                                // e.g., to use IgnoreQueryFilters for getting ALL users including soft-deleted ones.
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            // UserManager.Users returns an IQueryable. The global query filter in DbContext
            // (builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);)
            // will automatically exclude soft-deleted users.
            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            // UserManager.FindByIdAsync also respects global query filters.
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            // UserManager.FindByNameAsync also respects global query filters.
            return await _userManager.FindByNameAsync(username);
        }

        public async Task SoftDeleteUserAsync(string userId)
        {
            // Retrieve the user, potentially ignoring the global query filter if you need to "soft delete" an already soft-deleted user
            // or if you want to ensure you get the user regardless of its IsDeleted state.
            // For general soft-delete, FindByIdAsync is fine as it respects the filter.
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null && !user.IsDeleted) // Only soft delete if not already deleted
            {
                user.IsDeleted = true;
                var result = await _userManager.UpdateAsync(user); // Use UserManager's update
                if (!result.Succeeded)
                {
                    // Handle errors if update fails (e.g., log them, throw an exception)
                    throw new InvalidOperationException($"Failed to soft delete user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public async Task RestoreUserAsync(string userId)
        {
            // To restore, we must first retrieve the user, *ignoring* the global query filter.
            // This is where direct DbContext access is often useful.
            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && user.IsDeleted) // Only restore if currently deleted
            {
                user.IsDeleted = false;
                var result = await _userManager.UpdateAsync(user); // Use UserManager's update
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to restore user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public async Task HardDeleteUserAsync(string userId)
        {
            // IMPORTANT: Use with extreme caution! This permanently removes the user.
            // If you exclusively use soft delete, you might remove this method or secure it heavily.
            var user = await _userManager.FindByIdAsync(userId); // Fetches based on current filter

            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user); // UserManager's hard delete
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to hard delete user {userId}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            // Use UserManager's update for general user profile updates
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to update user {user.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}