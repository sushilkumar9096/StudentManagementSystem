using Student.Core.Entities;

namespace Student.Core.Interfaces
{
    public interface IStudentRepository : IRepository<Entities.Student>
    {
        Task<Entities.Student?> GetByEmailAsync(string email);
        Task<IEnumerable<Entities.Student>> SearchStudentsAsync(string? searchTerm, string? course);
    }
}
