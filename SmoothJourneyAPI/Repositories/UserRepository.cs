using SmoothJourneyAPI.Data;
using SmoothJourneyAPI.Interfaces;
using SmoothJourneyAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace SmoothJourneyAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SmoothJourneyDbContext _db;
        public UserRepository(SmoothJourneyDbContext db) => _db = db;

        public Task<Users?> GetByEmailAsync(string email) =>
            _db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email);

        public Task<Users?> GetByIdAsync(long id) => _db.Users.FindAsync(id).AsTask();

        public async Task<IEnumerable<Users>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            return await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Users user) => await _db.Users.AddAsync(user);

        public async Task DeleteAsync(Users user)
        {
            _db.Users.Remove(user);
            await Task.CompletedTask;
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
