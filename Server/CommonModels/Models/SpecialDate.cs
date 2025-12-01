using System;
using CommonModels.Interfaces;

namespace CommonModels.Models;

/// <summary>
/// Особая дата в календаре. Праздник или рабочий выходной.
/// </summary>
public class SpecialDate : IHasId
{
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Дата.
  /// </summary>
  public virtual DateTime Date { get; set; }
  
  /// <summary>
  /// Признак является ли день выходным.
  /// </summary>
  public virtual bool IsHoliday { get; set; }
}