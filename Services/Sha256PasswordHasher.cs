using System.Security.Cryptography;
using System.Text;

namespace ParbaudesDarbs.Api.Services;

public class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool Verify(string password, string passwordHash)
    {
        return Hash(password) == passwordHash;
    }
}
