using Student.Core.Entities;

namespace Student.Core.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string token, DateTime expiration) GenerateToken(User user);
    }
}
