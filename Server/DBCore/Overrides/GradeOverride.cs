using CommonModels.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DBCore.Overrides;

public class GradeOverride : IAutoMappingOverride<Grade>
{
  public void Override(AutoMapping<Grade> mapping)
  {
    mapping.Id(t => t.Id).GeneratedBy.Assigned();
  }
}