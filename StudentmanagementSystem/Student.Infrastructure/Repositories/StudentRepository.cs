using Microsoft.EntityFrameworkCore;
using Student.Core.Entities;
using Student.Core.Interfaces;
using Student.Infrastructure.Data;

namespace Student.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Core.Entities.Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Core.Entities.Student?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Core.Entities.Student>> SearchStudentsAsync(string? searchTerm, string? course)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(term) || s.Email.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(course))
            {
                var crs = course.Trim().ToLower();
                query = query.Where(s => s.Course.ToLower() == crs);
            }

            return await query.OrderByDescending(s => s.CreatedDate).ToListAsync();
        }
    }
}
