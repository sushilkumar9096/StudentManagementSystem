using Microsoft.EntityFrameworkCore;
using Student.Core.Entities;
using Student.Core.Interfaces;
using Student.Infrastructure.Data;

namespace Student.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var target = usernameOrEmail.Trim().ToLower();
            return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == target || u.Email.ToLower() == target);
        }
    }
}
