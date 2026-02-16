using ParbaudesDarbs.Api.Models;

namespace ParbaudesDarbs.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
