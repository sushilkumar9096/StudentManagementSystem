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
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            _authService = new AuthService(_mockUserRepository.Object, _mockJwtTokenGenerator.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnToken()
        {
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "Password123!",
                Role = "User"
            };

            _mockUserRepository.Setup(r => r.GetByUsernameAsync("newuser")).ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.GetByEmailAsync("newuser@test.com")).ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

            _mockJwtTokenGenerator.Setup(j => j.GenerateToken(It.IsAny<User>()))
                .Returns(("fake_jwt_token_string", DateTime.UtcNow.AddHours(2)));

            var response = await _authService.RegisterAsync(registerDto);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.StatusCode.Should().Be(201);
            response.Data!.Token.Should().Be("fake_jwt_token_string");
            response.Data.Username.Should().Be("newuser");
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ShouldThrowConflictException()
        {
            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Email = "user@test.com",
                Password = "Password123!"
            };

            _mockUserRepository.Setup(r => r.GetByUsernameAsync("existinguser")).ReturnsAsync(new User { Id = 1, Username = "existinguser" });

            Func<Task> act = async () => await _authService.RegisterAsync(registerDto);

            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("*already taken*");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthToken()
        {
            var rawPassword = "SecretPassword123!";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "testuser@test.com",
                PasswordHash = passwordHash,
                Role = "User"
            };

            var loginDto = new LoginDto
            {
                UsernameOrEmail = "testuser",
                Password = rawPassword
            };

            _mockUserRepository.Setup(r => r.GetByUsernameOrEmailAsync("testuser")).ReturnsAsync(user);
            _mockJwtTokenGenerator.Setup(j => j.GenerateToken(user))
                .Returns(("valid_jwt_token", DateTime.UtcNow.AddHours(2)));

            var response = await _authService.LoginAsync(loginDto);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data!.Token.Should().Be("valid_jwt_token");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = passwordHash
            };

            var loginDto = new LoginDto
            {
                UsernameOrEmail = "testuser",
                Password = "WrongPassword123!"
            };

            _mockUserRepository.Setup(r => r.GetByUsernameOrEmailAsync("testuser")).ReturnsAsync(user);

            Func<Task> act = async () => await _authService.LoginAsync(loginDto);

            await act.Should().ThrowAsync<Core.Exceptions.UnauthorizedAccessException>()
                .WithMessage("*Invalid username/email or password*");
        }
    }
}
