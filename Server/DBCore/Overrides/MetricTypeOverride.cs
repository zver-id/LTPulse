using CommonModels.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DBCore.Overrides;

public class MetricTypeOverride : IAutoMappingOverride<MetricType>
{
  public void Override(AutoMapping<MetricType> mapping)
  {
    mapping.Map(x => x.Name)
      .Unique();
  }
}