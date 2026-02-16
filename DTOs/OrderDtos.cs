using System.ComponentModel.DataAnnotations;

namespace ParbaudesDarbs.Api.DTOs;

public class OrderCreateRequest
{
    [MinLength(1)]
    public List<OrderItemCreateRequest> Items { get; set; } = new();
}

public class OrderItemCreateRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
}

public class OrderResponse
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
