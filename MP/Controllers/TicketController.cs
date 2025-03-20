using Microsoft.AspNetCore.Mvc;
using MP.Models;
using MP.Services;

namespace MP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    /// <summary>
    /// Gets tickets within a date range.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TicketViewModel[]>> GetTickets([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string timezone = "UTC")
    {
        try
        {
            var ticketViewModels = await _ticketService.GetTicketsAsync(from, to, timezone);
            return Ok(ticketViewModels);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in GetTickets: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets for range {From} to {To} in timezone {Timezone}.", from, to, timezone);
            return StatusCode(500, $"Error retrieving tickets: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves a new ticket.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<long>> SaveTicket([FromBody] TicketSaveModel arg)
    {
        try
        {
            var ticketId = await _ticketService.SaveTicketAsync(arg);
            return Ok(ticketId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in SaveTicket: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving ticket with VisitDate {VisitDate}.", arg.VisitDate);
            return StatusCode(500, $"Error saving ticket: {ex.Message}");
        }
    }
}