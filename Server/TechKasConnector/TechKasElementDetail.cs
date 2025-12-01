using System.Collections;

namespace TechKasConnector;

public class TechKasElementDetail : TechKasReferenceRecord, IEnumerable<TechKasElement>
{
  public TechKasElementDetail(dynamic detail)
  {
    this.Element = detail;
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
}