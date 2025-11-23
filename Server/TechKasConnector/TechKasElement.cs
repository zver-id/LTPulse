namespace TechKasConnector;

public class TechKasElement
{
  private dynamic Element { get; set; }
   
  public string GetRequisite(string requisite, RequisitesMode mode)
  {
    switch (mode)
    {
      case RequisitesMode.AsString:
        return this.Element.Requisite(requisite).AsString;
    }
    return string.Empty;
  }

  public TechKasElement(dynamic element)
  {
    this.Element = element;
  }
}