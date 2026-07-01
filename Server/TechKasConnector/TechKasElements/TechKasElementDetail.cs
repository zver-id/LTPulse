using System.Collections;

namespace TechKasConnector;

internal class TechKasElementDetail : TechKasReferenceRecord, IEnumerable<TechKasElement>, IDisposable
{
  public TechKasElementDetail(dynamic detail, dynamic parentElement)
  {
    this.Element = detail;
    this.ParentElement = parentElement;
  }
  
  private dynamic ParentElement { get; }
  
  /// <summary>
  /// Достигнут конец списка.
  /// </summary>
  /// <returns>Если true, то список завершился.</returns>
  public bool IsEndOfList()
  {
    return this.Element.EOF;
  }
  
  /// <summary>
  /// Установить указатель на первый элемент.
  /// </summary>
  public TechKasElement First()
  {
    this.Element.First();
    return new TechKasElement(this.Element, true);
  }
  
  /// <summary>
  /// Переключиться на следующий элемент.
  /// </summary>
  public TechKasElement Next()
  {
    this.Element.Next();
    return new TechKasElement(this.Element, true);
  }

  public IEnumerator<TechKasElement> GetEnumerator()
  {
    this.Element.First();
    while (!this.Element.EOF)
    {
      yield return new TechKasElement(this.Element, true);
      this.Element.Next();
    }
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return this.GetEnumerator();
  }

  public void Dispose()
  {
    this.Element.CloseRecord();
    this.ParentElement.CloseRecord();
  }
}