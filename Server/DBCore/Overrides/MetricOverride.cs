using Common.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping.Steps;

namespace DBCore.Overrides;

public class MetricOverride : IAutoMappingOverride<Metric>
{
  public void Override(AutoMapping<Metric> mapping)
  {
    //TODO нужно добавить в индекс столбцы команды и метрики возможно будет быстрее
    mapping.Map(x => x.Date)
      .Index("Ix_MetricDate");
    mapping.Map(x=>x.Date).Not.Nullable();
    mapping.References(x=>x.MetricType).Not.Nullable();
    mapping.References(x => x.Team).Not.Nullable();
    
    //TODO нужно ограничение на три столбца
  }
}