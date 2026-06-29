using CommonModels.Interfaces;
using CommonModels.Models;

namespace TechKasConnector.Calendar;

public class CalendarCalculator
{
  private IRepository Repository { get; }

  /// <summary>
  /// Праздники.
  /// </summary>
  private readonly Lazy<Task<List<SpecialDate>>> lazyHolidays;
  private async Task<bool> IsHoliday(DateTime date)
  {
    var holidays = await this.lazyHolidays.Value;
    return holidays.Select(x => x.Date)
      .Any(day => day.Date == date.Date);
  }

  /// <summary>
  /// Рабочие выходные.
  /// </summary>
  private readonly Lazy<Task<List<SpecialDate>>> lazyWorkingHolidays;
  private async Task<bool> IsWorkingHoliday(DateTime date)
  {
    var workingHolidays = await this.lazyWorkingHolidays.Value;
    return workingHolidays.Select(x => x.Date)
      .Any(day => day.Date == date.Date);
  }

  /// <summary>
  /// Возвращает список дат предшествующих дню расчета. Если предыдущий день выходной, то добавить и его.
  /// </summary>
  /// <param name="daysAgo">Количество дней.</param>
  /// <returns>Список дат.</returns>
  public async Task<List<string>> GetPreviousDates(int daysAgo)
  {
    var currentDay = DateTime.Now.Date - TimeSpan.FromDays(daysAgo);
    var previousDates = new List<DateTime>();
    previousDates.Add(currentDay);
    if (daysAgo != 0)
    {
      currentDay = currentDay.AddDays(-1);
      while (await this.IsHoliday(currentDay) ||
             (currentDay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday &&
              ! await this.IsWorkingHoliday(currentDay)))
      {
        previousDates.Add(currentDay);
        currentDay = currentDay.AddDays(-1);
      }
    }
    return previousDates
      .OrderBy(day => day.Date)
      .Select(d => d.ToString("dd.MM.yyyy"))
      .ToList();
  }

  /// <summary>
  /// Расчет количества рабочего времени в минутах.
  /// </summary>
  /// <param name="startDate">Начало периода.</param>
  /// <param name="endDate">Конец периода.</param>
  /// <returns>Количество минут рабочего времни в периоде.</returns>
  public async Task<int> GetDifferenceInMinutes(DateTime startDate, DateTime endDate)
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

      if ((isWeekday && isWorkingTime && !await this.IsHoliday(current)) ||
          (await this.IsWorkingHoliday(current) && isWorkingTime))
      {
        result++;
      }
      current = current.AddMinutes(1);
    }
    return result;
  }

  /// <summary>
  /// Получить следующий рабочий день.
  /// </summary>
  /// <param name="startDate">Время начала отсчета.</param>
  /// <param name="interval">Интервал времени.</param>
  /// <returns>Дата следующего рабочего дня. Время то же что и в начале.</returns>
  public async Task<DateTime> AddTimeSpanWithHolidays(DateTime startDate, TimeSpan interval)
  {
    while (true)
    {
      startDate = startDate.Add(interval);
      if (await this.IsWorkingHoliday(startDate.Date))
        return startDate;
      if (await this.IsHoliday(startDate.Date) || startDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        continue;
      return startDate;
    }
  }

  public CalendarCalculator(IRepository repository)
  {
    this.Repository = repository;
    this.lazyHolidays = new Lazy<Task<List<SpecialDate>>>(() => this.Repository.GetAsync<SpecialDate>(date => date.IsHoliday));
    this.lazyWorkingHolidays = new Lazy<Task<List<SpecialDate>>>(() => this.Repository.GetAsync<SpecialDate>(date => !date.IsHoliday));
  }
}