using TechKasConnector.Requisites;

namespace TechKasConnector;

public class TechKasElement : TechKasReferenceRecord
{
  /// <summary>
  /// Признак, что элемент является простым и не имеет вложенных коллекций.
  /// </summary>
  public bool IsSimple { get; set; }

  /// <summary>
  /// Гиперссылка на элемент
  /// </summary>
  public string Hyperlink => this.Element.Hyperlink;

  /// <summary>
  /// Получить вложенную коллекцию.
  /// </summary>
  /// <param name="detailNumber">ID коллекции.</param>
  /// <returns>Вложенная коллекция.</returns>
  /// <exception cref="InvalidOperationException">Возвращается в случае, если элемент не имеет вложенных коллекций.</exception>
  public TechKasElementDetail GetDetail(int detailNumber)
  {
    if (this.IsSimple)
      throw new InvalidOperationException("Element has no details");
    this.Element.OpenRecord();
    return new TechKasElementDetail(this.Element.DetailDataSet(detailNumber));
  }
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="element"></param>
  /// <param name="isSimple"></param>
  public TechKasElement(dynamic element, bool isSimple = false)
  {
    this.Element = element;
    this.IsSimple = isSimple;
  }
}