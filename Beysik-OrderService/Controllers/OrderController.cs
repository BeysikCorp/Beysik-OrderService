using Microsoft.AspNetCore.Mvc;
using Beysik_OrderService.Models;

namespace Beysik_OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly Services.OrderService _orderService;

    public OrderController(Services.OrderService orderService)
    {
        _orderService = orderService;

    }

    [HttpGet("/orders")]
    public async Task<IEnumerable<Order>> Get()
    {
        try
        {
            // Simulate async operation for demonstration; replace with real async DB call if available
            return await Task.Run(() => _orderService.Get());
        }
        catch (Exception)
        {
            // Log the exception (logging mechanism assumed)
            // _logger.LogError(ex, "An error occurred while retrieving orders.");
            return Enumerable.Empty<Order>();
        }
    }

    [HttpGet("/orders/id")]

    public async Task<ActionResult<Order>> GetByID([FromQuery]int orderId)
    {
        try
        {
            var order = await Task.Run(() => _orderService.Get(orderId));
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        catch (Exception)
        {
            // Log the exception (logging mechanism assumed)
            // _logger.LogError(ex, "An error occurred while retrieving the order.");
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpGet("/orders/product")]

    public async Task<ActionResult<string>> GetProduct([FromQuery] int orderId)
    {
        try
        {
            var order = await Task.Run(() => _orderService.Get(orderId));
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        catch (Exception)
        {
            // Log the exception (logging mechanism assumed)
            // _logger.LogError(ex, "An error occurred while retrieving the order.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("/orders")]
    public async Task<IActionResult> Post([FromBody] OrderRequest orderReq)
    {
        if (orderReq == null)
        {
            return BadRequest("Order cannot be null");
        }
        try
        {
            await Task.Run(() => _orderService.AddAsync(orderReq));
            return CreatedAtAction(nameof(Get), new { id = orderReq.UserID }, orderReq);
        }
        catch (Exception)
        {
            // Log the exception (logging mechanism assumed)
            // _logger.LogError(ex, "An error occurred while creating the order.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("/orders")]
    public async Task<IActionResult> Put([FromQuery] int id, [FromBody] Order order)
    {
        if (order == null || order.OrderID != id)
        {
            return BadRequest("Order cannot be null and must match the ID");
        }
        try
        {
            var existingOrder = await Task.Run(() => _orderService.Get(id));
            if (existingOrder == null)
            {
                return NotFound();
            }
            await Task.Run(() => _orderService.Update(order));
            return NoContent();
        }
        catch (Exception)
        {
            // Log the exception (logging mechanism assumed)
            // _logger.LogError(ex, "An error occurred while updating the order.");
            return StatusCode(500, "Internal server error");
        }
    }

}
