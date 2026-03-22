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

  public string GetRequisiteWithOpen(string requisite, RequisitesMode mode)
  {
    this.Element.OpenRecord();
    var value = this.GetRequisite(requisite, mode);
    this.Element.Cancel();
    this.Element.CloseRecord();
    return value;
  }
}