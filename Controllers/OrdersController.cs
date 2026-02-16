using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParbaudesDarbs.Api.Data;
using ParbaudesDarbs.Api.DTOs;
using ParbaudesDarbs.Api.Models;

namespace ParbaudesDarbs.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Atgriež pasūtījumus: Admin redz visus, klienti redz tikai savus.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll()
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var query = dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(o => o.UserId == userId);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderResponse
            {
                Id = o.Id,
                CreatedAtUtc = o.CreatedAtUtc,
                UserId = o.UserId,
                UserEmail = o.User!.Email,
                Total = o.Items.Sum(i => i.UnitPrice * i.Quantity),
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// Izveido jaunu pasūtījumu autorizētam lietotājam.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderResponse>> Create(OrderCreateRequest request)
    {
        if (request.Items.Count == 0)
        {
            return BadRequest("Pasūtījumā jābūt vismaz vienai precei.");
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
        {
            return BadRequest("Viena vai vairākas preces netika atrastas.");
        }

        var userId = GetUserId();

        var order = new Order
        {
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            Items = request.Items.Select(item =>
            {
                var product = products[item.ProductId];
                return new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };
            }).ToList()
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        var userEmail = await dbContext.Users.Where(u => u.Id == userId).Select(u => u.Email).FirstAsync();
        var response = new OrderResponse
        {
            Id = order.Id,
            CreatedAtUtc = order.CreatedAtUtc,
            UserId = userId,
            UserEmail = userEmail,
            Total = order.Items.Sum(i => i.Quantity * i.UnitPrice),
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = products[i.ProductId].Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            }).ToList()
        };

        return CreatedAtAction(nameof(GetAll), new { id = order.Id }, response);
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(ClaimTypes.Name)
                  ?? User.FindFirstValue("sub")
                  ?? User.FindFirstValue(ClaimTypes.Sid)
                  ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Neizdevās noteikt lietotāja identitāti no tokena.");
        }

        return userId;
    }
}
