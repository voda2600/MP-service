namespace MP.Domain;

public interface ITicketManager
{
    public List<Ticket> GetTickets();

    public void AddTicket(Ticket? ticket);
}