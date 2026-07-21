using Student.Core.Common;
using Student.Core.DTOs;
using Student.Core.Entities;
using Student.Core.Exceptions;
using Student.Core.Interfaces;

namespace Student.Core.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<ApiResponse<IEnumerable<StudentDto>>> GetAllStudentsAsync(string? searchTerm = null, string? course = null)
        {
            var students = await _studentRepository.SearchStudentsAsync(searchTerm, course);
            var dtos = students.Select(MapToDto);
            return ApiResponse<IEnumerable<StudentDto>>.SuccessResponse(dtos, "Students retrieved successfully");
        }

        public async Task<ApiResponse<StudentDto>> GetStudentByIdAsync(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                throw new NotFoundException($"Student with ID {id} was not found.");
            }

            return ApiResponse<StudentDto>.SuccessResponse(MapToDto(student), "Student retrieved successfully");
        }

        public async Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentDto createDto)
        {
            // Check for duplicate email
            var existingStudent = await _studentRepository.GetByEmailAsync(createDto.Email);
            if (existingStudent != null)
            {
                throw new ConflictException($"A student with email '{createDto.Email}' already exists.");
            }

            var student = new Entities.Student
            {
                Name = createDto.Name.Trim(),
                Email = createDto.Email.Trim().ToLowerInvariant(),
                Age = createDto.Age,
                Course = createDto.Course.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            var createdStudent = await _studentRepository.AddAsync(student);
            return ApiResponse<StudentDto>.SuccessResponse(MapToDto(createdStudent), "Student created successfully", 201);
        }

        public async Task<ApiResponse<StudentDto>> UpdateStudentAsync(int id, UpdateStudentDto updateDto)
        {
            var existingStudent = await _studentRepository.GetByIdAsync(id);
            if (existingStudent == null)
            {
                throw new NotFoundException($"Student with ID {id} was not found.");
            }

            // Check if email belongs to another student
            var studentWithEmail = await _studentRepository.GetByEmailAsync(updateDto.Email);
            if (studentWithEmail != null && studentWithEmail.Id != id)
            {
                throw new ConflictException($"A student with email '{updateDto.Email}' already exists.");
            }

            existingStudent.Name = updateDto.Name.Trim();
            existingStudent.Email = updateDto.Email.Trim().ToLowerInvariant();
            existingStudent.Age = updateDto.Age;
            existingStudent.Course = updateDto.Course.Trim();

            await _studentRepository.UpdateAsync(existingStudent);
            return ApiResponse<StudentDto>.SuccessResponse(MapToDto(existingStudent), "Student updated successfully");
        }

        public async Task<ApiResponse<bool>> DeleteStudentAsync(int id)
        {
            var existingStudent = await _studentRepository.GetByIdAsync(id);
            if (existingStudent == null)
            {
                throw new NotFoundException($"Student with ID {id} was not found.");
            }

            await _studentRepository.DeleteAsync(existingStudent);
            return ApiResponse<bool>.SuccessResponse(true, "Student deleted successfully");
        }

        private static StudentDto MapToDto(Entities.Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Age = student.Age,
                Course = student.Course,
                CreatedDate = student.CreatedDate
            };
        }
    }
}
