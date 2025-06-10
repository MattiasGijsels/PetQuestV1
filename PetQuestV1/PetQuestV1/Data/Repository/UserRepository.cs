using Microsoft.EntityFrameworkCore;
using PetQuestV1.Data; 
using PetQuestV1.Data.Defines; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetQuestV1.Data.Repository 
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
                                    .Include(u => u.Pets) 
                                    .ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string userId)
        {
            using var _dbContext = _dbContextFactory.CreateDbContext();
            return await _dbContext.Users
                                    .Include(u => u.Pets) 
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
                _dbContext.Users.Update(user); 
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
                _dbContext.Users.Update(user); 
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}