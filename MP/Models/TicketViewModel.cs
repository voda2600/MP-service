namespace MP.Models;

/// <summary>
///     Заявка на посещение
/// </summary>
[Serializable]
public class TicketViewModel
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public long? ID { get; set; }

    /// <summary>
    ///     Дата создания
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    ///     Название
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Описание
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Дата посещения
    /// </summary>
    public DateTimeOffset VisitDate { get; set; }

    /// <summary>
    ///     Количество людей
    /// </summary>
    public int VisitorsNumber { get; set; }
}