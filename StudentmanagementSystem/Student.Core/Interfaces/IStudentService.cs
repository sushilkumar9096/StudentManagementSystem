using Student.Core.Common;
using Student.Core.DTOs;

namespace Student.Core.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResponse<IEnumerable<StudentDto>>> GetAllStudentsAsync(string? searchTerm = null, string? course = null);
        Task<ApiResponse<StudentDto>> GetStudentByIdAsync(int id);
        Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentDto createDto);
        Task<ApiResponse<StudentDto>> UpdateStudentAsync(int id, UpdateStudentDto updateDto);
        Task<ApiResponse<bool>> DeleteStudentAsync(int id);
    }
}
