using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Student.Api.Controllers;
using Student.Core.Common;
using Student.Core.DTOs;
using Student.Core.Interfaces;
using Xunit;

namespace Student.Tests
{
    public class StudentsControllerTests
    {
        private readonly Mock<IStudentService> _mockStudentService;
        private readonly StudentsController _controller;

        public StudentsControllerTests()
        {
            _mockStudentService = new Mock<IStudentService>();
            _controller = new StudentsController(_mockStudentService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkResultWithStudents()
        {
            // Arrange
            var students = new List<StudentDto>
            {
                new StudentDto { Id = 1, Name = "Aarav", Email = "aarav@test.com", Age = 20, Course = "CS" }
            };

            var serviceResult = ApiResponse<IEnumerable<StudentDto>>.SuccessResponse(students);
            _mockStudentService.Setup(s => s.GetAllStudentsAsync(It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetAll(null, null);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<StudentDto>>>().Subject;
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetById_WhenStudentExists_ShouldReturnOkResult()
        {
            // Arrange
            var student = new StudentDto { Id = 1, Name = "Aarav", Email = "aarav@test.com", Age = 20, Course = "CS" };
            var serviceResult = ApiResponse<StudentDto>.SuccessResponse(student);
            _mockStudentService.Setup(s => s.GetStudentByIdAsync(1)).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<StudentDto>>().Subject;
            apiResponse.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Create_WithValidDto_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var createDto = new CreateStudentDto { Name = "Rohan", Email = "rohan@test.com", Age = 22, Course = "IT" };
            var studentDto = new StudentDto { Id = 5, Name = "Rohan", Email = "rohan@test.com", Age = 22, Course = "IT" };
            var serviceResult = ApiResponse<StudentDto>.SuccessResponse(studentDto, "Created", 201);

            _mockStudentService.Setup(s => s.CreateStudentAsync(createDto)).ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdAtResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtResult.StatusCode.Should().Be(201);
            createdAtResult.RouteValues!["id"].Should().Be(5);
        }
    }
}
