using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParbaudesDarbs.Api.Data;
using ParbaudesDarbs.Api.DTOs;
using ParbaudesDarbs.Api.Models;
using ParbaudesDarbs.Api.Services;

namespace ParbaudesDarbs.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext dbContext, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService) : ControllerBase
{
    /// <summary>
    /// Reģistrē jaunu lietotāju ar e-pastu, paroli un lomu.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await dbContext.Users.AnyAsync(u => u.Email == email))
        {
            return BadRequest("Lietotājs ar šādu e-pastu jau eksistē.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = string.IsNullOrWhiteSpace(request.Role) ? "Customer" : request.Role.Trim()
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var (token, expiresAt) = jwtTokenService.CreateToken(user);
        return CreatedAtAction(nameof(Register), new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAt,
            Email = user.Email,
            Role = user.Role
        });
    }

    /// <summary>
    /// Autentificē lietotāju un atgriež JWT tokenu.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Nepareizs e-pasts vai parole.");
        }

        var (token, expiresAt) = jwtTokenService.CreateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAt,
            Email = user.Email,
            Role = user.Role
        });
    }
}
