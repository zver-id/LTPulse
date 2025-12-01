using CommonModels.Models;
using DBCore;

namespace TechKasConnector.Calendar;

public class CalendarCalculator
{
  public DBRepository Repository { get; set; }
  
  /// <summary>
  /// Возвращает список дат предшествующих дню расчета. Если предыдущий день выходной, то добавить и его.
  /// </summary>
  /// <param name="daysAgo">Количество дней.</param>
  /// <returns>Список дат.</returns>
  public List<string> GetPreviousDates(int daysAgo)
  {
    var currentDay = DateTime.Now.Date - TimeSpan.FromDays(daysAgo);
    var previousDates = new List<DateTime>();
    previousDates.Add(currentDay);
    if (daysAgo != 0)
    {
      var holidays = this.Repository.GetByPredicate<SpecialDate>(date => date.IsHoliday)
        .Select(x => x.Date).ToList();
      var workingDays = this.Repository.GetByPredicate<SpecialDate>(date => !date.IsHoliday)
        .Select(x => x.Date).ToList();
      currentDay = currentDay.AddDays(-1);
      while (holidays.Contains(currentDay) ||
             (currentDay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday && !workingDays.Contains(currentDay)))
      {
        previousDates.Add(currentDay);
        currentDay = currentDay.AddDays(-1);
      }
    }
    return previousDates.Select(d => d.ToString("dd.MM.yyyy")).ToList();
  }

  public CalendarCalculator(DBRepository repository)
  {
    this.Repository = repository;
  }
}