using MetaExcange.Console.Models;
using MetaExcange.Console.Services;
using MetaExchange.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace MetaExchange.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionController : ControllerBase
{
    private readonly BestExecutionService _service;

    public ExecutionController()
    {
        _service = new BestExecutionService();
    }

    [HttpPost]
    public IActionResult Execute([FromBody] ExecutionRequestDto request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        if (request.AmountBtc <= 0)
        {
            return BadRequest(new { error = "AmountBtc must be greater than 0." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
            
        try
        {
            var basePath = AppContext.BaseDirectory;

            var orderBooksPath = Path.Combine(basePath, "Data", "orderbooks.json");
            var balancesPath = Path.Combine(basePath, "Data", "balances.json");

            var orderBookLines = System.IO.File.ReadAllText(orderBooksPath);
            var orderBooks = JsonSerializer.Deserialize<List<OrderBook>>(orderBookLines);
            if (orderBooks == null || orderBooks.Count == 0)
            {
                return BadRequest("Order books file is empty or invalid.");
            }

            var balancesJson = System.IO.File.ReadAllText(balancesPath);
            var balances = JsonSerializer.Deserialize<List<ExchangeBalance>>(balancesJson);
            if (balances == null || balances.Count == 0)
            {
                return BadRequest("Balances file is empty or invalid.");
            }

            var orders = _service.Execute(
                orderBooks,
                balances,
                request.Side,
                request.AmountBtc);

            var totalEur = orders.Sum(o => o.AmountBtc * o.Price);

            return Ok(new ExecutionResponseDto
            {
                Orders = orders,
                TotalEur = totalEur
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
