using Student.Core.Entities;

namespace Student.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    }
}
