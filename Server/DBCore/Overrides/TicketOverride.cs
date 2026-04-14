using CommonModels.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DBCore.Overrides;

public class TicketOverride : IAutoMappingOverride<Ticket>
{
  public void Override(AutoMapping<Ticket> mapping)
  {
    mapping.Id(t => t.Id).GeneratedBy.Assigned();
  }
}