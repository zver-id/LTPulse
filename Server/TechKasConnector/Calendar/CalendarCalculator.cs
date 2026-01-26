using System.Runtime.CompilerServices;
using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;

namespace TechKasConnector.Calendar;

public class CalendarCalculator
{
  public IRepository Repository { get; set; }

  /// <summary>
  /// Праздники.
  /// </summary>
  private List<DateTime> Holidays { get; }
  private bool isHoliday(DateTime date) => this.Holidays.Any(day => day.Date == date.Date);
  
  /// <summary>
  /// Рабочие выходные.
  /// </summary>
  private List<DateTime> WorkingHolidays { get; }
  private bool isWorkingHoliday(DateTime date) => this.WorkingHolidays.Any(day => day.Date == date.Date);
  
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
      currentDay = currentDay.AddDays(-1);
      while (this.isHoliday(currentDay) ||
             (currentDay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday &&
              !this.isWorkingHoliday(currentDay)))
      {
        previousDates.Add(currentDay);
        currentDay = currentDay.AddDays(-1);
      }
    }
    return previousDates.Select(d => d.ToString("dd.MM.yyyy")).ToList();
  }

  /// <summary>
  /// Расчет количества рабочего времени в минутах.
  /// </summary>
  /// <param name="startDate">Начало периода.</param>
  /// <param name="endDate">Конец периода.</param>
  /// <returns>Количество минут рабочего времни в периоде.</returns>
  public int GetDifferenceInMinutes(DateTime startDate, DateTime endDate)
  {
    int result = 0;
    TimeSpan startOfWork = new TimeSpan(9, 0, 0);
    TimeSpan endOfWork = new TimeSpan(17, 0, 0);

    DateTime current = startDate;

    while (current <= endDate)
    {
      bool isWeekday = current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);
      
      TimeSpan currentTime = current.TimeOfDay;
      bool isWorkingTime = currentTime >= startOfWork && currentTime <= endOfWork;

      if ((isWeekday && isWorkingTime && !this.isHoliday(current)) ||
          (this.isWorkingHoliday(current) && isWorkingTime))
      {
        result++;
      }
      current = current.AddMinutes(1);
    }
    return result;
  }

  public CalendarCalculator(IRepository repository)
  {
    this.Repository = repository;
    this.Holidays = this.Repository.Get<SpecialDate>(date => date.IsHoliday)
      .Select(x => x.Date).ToList();
    this.WorkingHolidays = this.Repository.Get<SpecialDate>(date => !date.IsHoliday)
      .Select(x => x.Date).ToList();
  }
}