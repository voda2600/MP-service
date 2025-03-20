namespace MP.Models;

/// <summary>
///     Заявка на посещение
/// </summary>
[Serializable]
public class TicketSaveModel
{
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