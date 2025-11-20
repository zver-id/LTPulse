using CommonModels.Interfaces;

namespace CommonModels.Models;

public class TechKasFilter : IHasId
{
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Фильтруемое поле.
  /// </summary>
  public virtual string NameOfField { get; set; }
  
  /// <summary>
  /// Значение поля.
  /// </summary>
  public virtual string Value { get; set; }
  
  /// <summary>
  /// Признак фильтрации. True-нужно фильтровать по значению, False-нужно исключить значение.
  /// </summary>
  public virtual bool ShouldInclude { get; set; }
  
  /// <summary>
  /// Команда, к которой нужно применить фильтр. Если значение пустое - применить ко всем командам.
  /// </summary>
  public virtual Team? Team { get; set; }
}