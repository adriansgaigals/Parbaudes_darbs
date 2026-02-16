using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParbaudesDarbs.Api.Data;
using ParbaudesDarbs.Api.DTOs;
using ParbaudesDarbs.Api.Models;

namespace ParbaudesDarbs.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Atgriež visu preču sarakstu ar kategorijām.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
    {
        var products = await dbContext.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category!.Name
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Pievieno jaunu preci (pieejams tikai Admin lomai).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> Create(ProductCreateRequest request)
    {
        var category = await dbContext.Categories.FindAsync(request.CategoryId);
        if (category is null)
        {
            return BadRequest("Norādītā kategorija neeksistē.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = category.Name
        };

        return CreatedAtAction(nameof(GetAll), new { id = product.Id }, response);
    }
}
