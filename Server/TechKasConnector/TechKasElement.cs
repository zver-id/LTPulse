using TechKasConnector.Requisites;

namespace TechKasConnector;

public class TechKasElement : TechKasReferenceRecord
{
  public bool IsSimple { get; set; }
  public TechKasElementDetail GetDetail(int detailNumber)
  {
    if (this.IsSimple)
      throw new InvalidOperationException("Element has no details");
    this.Element.OpenRecord();
    return new TechKasElementDetail(this.Element.DetailDataSet(detailNumber));
  }
  
  public TechKasElement(dynamic element, bool isSimple = false)
  {
    this.Element = element;
    this.IsSimple = isSimple;
  }
}