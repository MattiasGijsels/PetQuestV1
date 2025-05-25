// PetQuestV1.Data.Repository/UserRepository.cs (or PetQuestV1.Repositories/UserRepository.cs)
using Microsoft.EntityFrameworkCore;
using PetQuestV1.Data; // For ApplicationUser and ApplicationDbContext
using PetQuestV1.Data.Defines; // For IUserRepository
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Data.Repository // Or your chosen namespace for repository implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public UserRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.Users
                                    .Include(u => u.Pets) // Include Pets if you need pet counts in service layer
                                    .ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.Users
                                    .Include(u => u.Pets) // Include Pets if you need pet counts in service layer
                                    .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetByUsernameAsync(string username)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task AddAsync(ApplicationUser user)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            // Attach the entity if it's detached (e.g., if it came from a DTO and was newed up)
            // Or use Update directly if it's already being tracked or has its properties set on an existing entity
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(string userId)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(string userId)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsDeleted = true;
                _dbContext.Users.Update(user); // Mark as modified
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(string userId)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsDeleted = false;
                _dbContext.Users.Update(user); // Mark as modified
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}