using CommonModels.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DBCore.Overrides;

public class TicketOverride : IAutoMappingOverride<Ticket>
{
  private const int NameLength = 4000;

  public void Override(AutoMapping<Ticket> mapping)
  {
    mapping.Id(t => t.Id).GeneratedBy.Assigned();
    // В данных ТехКас названия обращений могут превышать varchar(255).
    mapping.Map(t => t.Name).Length(NameLength);
    mapping.Map(t => t.DevsEscalationsData).Length(NameLength);
    mapping.Map(t => t.LineEscalationsData).Length(NameLength);
  }
}