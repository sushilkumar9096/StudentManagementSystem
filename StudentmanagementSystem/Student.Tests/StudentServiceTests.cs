using FluentAssertions;
using Moq;
using Student.Core.DTOs;
using Student.Core.Entities;
using Student.Core.Exceptions;
using Student.Core.Interfaces;
using Student.Core.Services;
using Xunit;

namespace Student.Tests
{
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _mockStudentRepository;
        private readonly StudentService _studentService;

        public StudentServiceTests()
        {
            _mockStudentRepository = new Mock<IStudentRepository>();
            _studentService = new StudentService(_mockStudentRepository.Object);
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnListOfStudents()
        {
            var students = new List<Core.Entities.Student>
            {
                new Core.Entities.Student { Id = 1, Name = "Aarav Sharma", Email = "aarav@test.com", Age = 21, Course = "Computer Science", CreatedDate = DateTime.UtcNow },
                new Core.Entities.Student { Id = 2, Name = "Priya Patel", Email = "priya@test.com", Age = 22, Course = "IT", CreatedDate = DateTime.UtcNow }
            };

            _mockStudentRepository.Setup(r => r.SearchStudentsAsync(It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(students);

            var response = await _studentService.GetAllStudentsAsync();

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Data!.First().Name.Should().Be("Aarav Sharma");
        }

        [Fact]
        public async Task GetStudentByIdAsync_WhenStudentExists_ShouldReturnStudentDto()
        {
            var student = new Core.Entities.Student
            {
                Id = 1,
                Name = "Aarav Sharma",
                Email = "aarav@test.com",
                Age = 21,
                Course = "Computer Science",
                CreatedDate = DateTime.UtcNow
            };

            _mockStudentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);

            var response = await _studentService.GetStudentByIdAsync(1);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data!.Id.Should().Be(1);
            response.Data.Name.Should().Be("Aarav Sharma");
        }

        [Fact]
        public async Task GetStudentByIdAsync_WhenStudentDoesNotExist_ShouldThrowNotFoundException()
        {
            _mockStudentRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Core.Entities.Student?)null);

            Func<Task> act = async () => await _studentService.GetStudentByIdAsync(99);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*99*not found*");
        }

        [Fact]
        public async Task CreateStudentAsync_WithValidData_ShouldCreateStudent()
        {
            var createDto = new CreateStudentDto
            {
                Name = "Vikram Singh",
                Email = "vikram@test.com",
                Age = 23,
                Course = "Data Science"
            };

            _mockStudentRepository.Setup(r => r.GetByEmailAsync("vikram@test.com")).ReturnsAsync((Core.Entities.Student?)null);
            _mockStudentRepository.Setup(r => r.AddAsync(It.IsAny<Core.Entities.Student>()))
                .ReturnsAsync((Core.Entities.Student s) => { s.Id = 10; return s; });

            var response = await _studentService.CreateStudentAsync(createDto);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.StatusCode.Should().Be(201);
            response.Data!.Id.Should().Be(10);
            response.Data.Name.Should().Be("Vikram Singh");
            _mockStudentRepository.Verify(r => r.AddAsync(It.IsAny<Core.Entities.Student>()), Times.Once);
        }

        [Fact]
        public async Task CreateStudentAsync_WithDuplicateEmail_ShouldThrowConflictException()
        {
            var createDto = new CreateStudentDto
            {
                Name = "Vikram Singh",
                Email = "existing@test.com",
                Age = 23,
                Course = "Data Science"
            };

            var existingStudent = new Core.Entities.Student { Id = 1, Email = "existing@test.com" };
            _mockStudentRepository.Setup(r => r.GetByEmailAsync("existing@test.com")).ReturnsAsync(existingStudent);

            Func<Task> act = async () => await _studentService.CreateStudentAsync(createDto);

            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task UpdateStudentAsync_WithValidData_ShouldUpdateStudent()
        {
            var existingStudent = new Core.Entities.Student
            {
                Id = 1,
                Name = "Aarav Sharma",
                Email = "aarav@test.com",
                Age = 21,
                Course = "Computer Science"
            };

            var updateDto = new UpdateStudentDto
            {
                Name = "Aarav Sharma Updated",
                Email = "aarav.updated@test.com",
                Age = 22,
                Course = "AI & ML"
            };

            _mockStudentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingStudent);
            _mockStudentRepository.Setup(r => r.GetByEmailAsync("aarav.updated@test.com")).ReturnsAsync((Core.Entities.Student?)null);

            var response = await _studentService.UpdateStudentAsync(1, updateDto);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data!.Name.Should().Be("Aarav Sharma Updated");
            response.Data.Course.Should().Be("AI & ML");
            _mockStudentRepository.Verify(r => r.UpdateAsync(It.IsAny<Core.Entities.Student>()), Times.Once);
        }

        [Fact]
        public async Task DeleteStudentAsync_WhenStudentExists_ShouldDeleteStudent()
        {
            var existingStudent = new Core.Entities.Student { Id = 1, Name = "Aarav Sharma" };
            _mockStudentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingStudent);

            var response = await _studentService.DeleteStudentAsync(1);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().BeTrue();
            _mockStudentRepository.Verify(r => r.DeleteAsync(existingStudent), Times.Once);
        }
    }
}
