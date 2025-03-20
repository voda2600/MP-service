using MP.Models;

namespace MP.Services;

public interface ITicketService
{
    Task<TicketViewModel[]> GetTicketsAsync(DateTime from, DateTime to, string timezone);
    Task<long> SaveTicketAsync(TicketSaveModel model);
}