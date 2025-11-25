using SmoothJourneyAPI.Models;

namespace SmoothJourneyAPI.Interfaces
{
    public interface IUserRepository
    {
        Task<Users?> GetByEmailAsync(string email);
        Task<Users?> GetByIdAsync(long id);
        Task<IEnumerable<Users>> GetAllAsync(int page = 1, int pageSize = 50);
        Task AddAsync(Users user);
        Task DeleteAsync(Users user);
        Task SaveChangesAsync();
    }
}
