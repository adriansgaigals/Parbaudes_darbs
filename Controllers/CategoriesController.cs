using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParbaudesDarbs.Api.Data;
using ParbaudesDarbs.Api.DTOs;
using ParbaudesDarbs.Api.Models;

namespace ParbaudesDarbs.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Atgriež visas preču kategorijas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll()
    {
        var categories = await dbContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Izveido jaunu kategoriju (pieejams tikai Admin lomai).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Category>> Create(CategoryCreateRequest request)
    {
        var exists = await dbContext.Categories.AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower());
        if (exists)
        {
            return BadRequest("Kategorija ar šādu nosaukumu jau eksistē.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, category);
    }
}
