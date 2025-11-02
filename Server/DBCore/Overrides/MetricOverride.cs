using CommonModels.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace DBCore.Overrides;

public class MetricOverride : IAutoMappingOverride<Metric>
{
  public void Override(AutoMapping<Metric> mapping)
  {
    //TODO нужно добавить в индекс столбцы команды и метрики возможно будет быстрее
    mapping.Map(x => x.Date).Index("Ix_MetricDate");
  }
}