using System.ComponentModel.DataAnnotations;

namespace ParbaudesDarbs.Api.DTOs;

public class ProductCreateRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999)]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
