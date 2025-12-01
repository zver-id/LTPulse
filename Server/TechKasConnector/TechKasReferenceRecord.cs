using TechKasConnector.Requisites;

namespace TechKasConnector;

public abstract class TechKasReferenceRecord
{
  protected dynamic Element { get; set; }
   
  public string GetRequisite(string requisite, RequisitesMode mode)
  {
    switch (mode)
    {
      case RequisitesMode.AsString:
        return this.Element.Requisites(requisite).AsString;
      case RequisitesMode.DisplayText:
        return this.Element.Requisites(requisite).DisplayText;
    }
    return string.Empty;
  }
}