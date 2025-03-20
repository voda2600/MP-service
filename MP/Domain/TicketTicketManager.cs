namespace MP.Domain;

/// <summary>
///     Менеджер для работы с БД
///     Синглтон-заглушка, чтоб не мудрить с базой ради тестового задания
/// </summary>
public class TicketTicketManager : ITicketManager
{
    private readonly string _context;
    private readonly object _lock;
    private readonly Dictionary<string, List<Ticket>> _tickets;

    public TicketTicketManager(string context)
    {
        _context = context;
        _tickets = new Dictionary<string, List<Ticket>> { { _context, new List<Ticket>() } };
        _lock = new object();
    }

    /// <summary>
    ///     Возвращаем список. Так как эмулируем базу, то будем тут клонировать, чтоб оригинал не трогать
    /// </summary>
    /// <returns></returns>
    public List<Ticket> GetTickets()
    {
        List<Ticket> res;

        lock (_lock)
        {
            res = _tickets[_context].Select(t => new Ticket
            {
                ID = t.ID,
                Title = t.Title,
                CreationDate = t.CreationDate,
                Description = t.Description,
                VisitDate = t.VisitDate,
                VisitorsNumber = t.VisitorsNumber
            }).ToList();
        }

        return res;
    }

    /// <summary>
    ///     Добавляем тоже как типовое поведение всех причуд для работы с базой
    /// </summary>
    /// <param name="ticket"></param>
    public void AddTicket(Ticket? ticket)
    {
        if (ticket == null || ticket.ID.HasValue)
            return;

        lock (_lock)
        {
            ticket.ID = _tickets.Count;

            _tickets[_context].Add(new Ticket
            {
                ID = ticket.ID,
                Title = ticket.Title,
                VisitDate = ticket.VisitDate,
                VisitorsNumber = ticket.VisitorsNumber,
                Description = ticket.Description,
                CreationDate = DateTime.UtcNow
            });
        }
    }
}