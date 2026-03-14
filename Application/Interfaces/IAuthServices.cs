using Domain.Entities;

namespace Application.Interfaces
{
    /// <summary>BCrypt password hashing contract.</summary>
    public interface IPasswordHasher
    {
        string Hash(string plainPassword);
        bool Verify(string plainPassword, string hash);
    }

    /// <summary>JWT token generation contract.</summary>
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        int ExpiresInSeconds { get; }
    }
}
