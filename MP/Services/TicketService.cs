using MP.Domain;
using MP.Models;

namespace MP.Services;

public class TicketService : ITicketService
{
    private readonly ITicketManager _ticketManager;
    private readonly ILogger<TicketService> _logger;

    public TicketService(ITicketManager ticketManager, ILogger<TicketService> logger)
    {
        _ticketManager = ticketManager;
        _logger = logger;
    }

    public Task<TicketViewModel[]> GetTicketsAsync(DateTime from, DateTime to, string timezone)
    {
        if (from > to)
        {
            _logger.LogWarning("Invalid date range: 'from' date ({From}) is greater than 'to' date ({To}).", from, to);
            throw new ArgumentException("Invalid date range: 'from' date must be less than or equal to 'to' date.");
        }
        
        TimeZoneInfo timeZoneInfo;
        try
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            _logger.LogWarning("Invalid timezone provided: {Timezone}. Defaulting to UTC.", timezone);
            timeZoneInfo = TimeZoneInfo.Utc;
        }
        
        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(from, timeZoneInfo);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(to, timeZoneInfo);
        
        var tickets = _ticketManager.GetTickets()
            .Where(t => t.VisitDate >= fromUtc && t.VisitDate <= toUtc)
            .ToArray();

        var ticketViewModels = tickets.Select(t => new TicketViewModel
        {
            ID = t.ID ?? -1,
            Title = t.Title,
            Description = t.Description,
            VisitDate = new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(t.VisitDate, timeZoneInfo), timeZoneInfo.BaseUtcOffset),
            VisitorsNumber = t.VisitorsNumber
        }).ToArray();

        _logger.LogInformation("Successfully retrieved {Count} tickets for range {From} to {To} in timezone {Timezone}.", ticketViewModels.Length, from, to, timezone);
        return Task.FromResult(ticketViewModels);
    }

    public Task<long> SaveTicketAsync(TicketSaveModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
        {
            _logger.LogWarning("Empty title provided in SaveTicket request.");
            throw new ArgumentException("Empty title");
        }

        if (model.VisitorsNumber < 1 || model.VisitorsNumber > 10)
        {
            _logger.LogWarning("Invalid VisitorsNumber provided: {VisitorsNumber}. Must be in range [1..10].", model.VisitorsNumber);
            throw new ArgumentException("VisitorsNumber value is not allowed. Value must be in interval [1..10]");
        }
        
        TimeZoneInfo timeZoneInfo;
        if (model.VisitDate.Offset != TimeSpan.Zero)
        {
            timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(
                id: $"CustomOffset_{model.VisitDate.Offset.TotalHours}",
                baseUtcOffset: model.VisitDate.Offset,
                displayName: $"Custom Offset {model.VisitDate.Offset}",
                standardDisplayName: $"Custom Offset {model.VisitDate.Offset}"
            );
            _logger.LogInformation("Determined timezone offset {Offset} from VisitDate.", model.VisitDate.Offset);
        }
        else
        {
            _logger.LogWarning("No timezone offset provided in VisitDate. Defaulting to UTC.");
            timeZoneInfo = TimeZoneInfo.Utc;
        }
        
        var visitDateUtc = TimeZoneInfo.ConvertTimeToUtc(model.VisitDate.DateTime, timeZoneInfo);
        
        if (visitDateUtc < DateTime.UtcNow)
        {
            _logger.LogWarning("Attempted to create a ticket with VisitDate in the past: {VisitDate}.", model.VisitDate);
            throw new ArgumentException("Ticket creation in the past is disabled");
        }

        var ticket = new Ticket
        {
            Title = model.Title,
            Description = model.Description,
            VisitDate = visitDateUtc,
            VisitorsNumber = model.VisitorsNumber,
            CreationDate = DateTime.UtcNow
        };
        
        _ticketManager.AddTicket(ticket);

        _logger.LogInformation("Successfully saved ticket with ID {TicketId} for VisitDate {VisitDate} (UTC: {VisitDateUtc}).", ticket.ID, model.VisitDate, visitDateUtc);
        return Task.FromResult(ticket.ID ?? -1);
    }
}