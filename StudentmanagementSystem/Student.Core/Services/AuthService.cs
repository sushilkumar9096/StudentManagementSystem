using Student.Core.Common;
using Student.Core.DTOs;
using Student.Core.Entities;
using Student.Core.Exceptions;
using Student.Core.Interfaces;

namespace Student.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(registerDto.Username.Trim());
            if (existingUserByUsername != null)
            {
                throw new ConflictException($"Username '{registerDto.Username}' is already taken.");
            }

            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email.Trim());
            if (existingUserByEmail != null)
            {
                throw new ConflictException($"Email '{registerDto.Email}' is already registered.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username.Trim(),
                Email = registerDto.Email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                Role = string.IsNullOrWhiteSpace(registerDto.Role) ? "User" : registerDto.Role.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var (token, expiration) = _jwtTokenGenerator.GenerateToken(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Expiration = expiration
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "User registered successfully", 201);
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail.Trim());
            if (user == null)
            {
                throw new Exceptions.UnauthorizedAccessException("Invalid username/email or password.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new Exceptions.UnauthorizedAccessException("Invalid username/email or password.");
            }

            var (token, expiration) = _jwtTokenGenerator.GenerateToken(user);

            var authResponse = new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Expiration = expiration
            };

            return ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful");
        }
    }
}
