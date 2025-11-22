using SmoothJourneyAPI.Interfaces;
using SmoothJourneyAPI.Models;

namespace SmoothJourneyAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task AddAsync(Users user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Users user)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Users>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            throw new NotImplementedException();
        }

        public Task<Users?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Users?> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
