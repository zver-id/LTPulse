using CommonModels.Models;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping.Steps;
using FluentNHibernate.Utils;

namespace DBCore.Overrides;

public class MetricOverride : IAutoMappingOverride<Metric>
{
  public void Override(AutoMapping<Metric> mapping)
  {
    //TODO нужно добавить в индекс столбцы команды и метрики возможно будет быстрее
    
    mapping.Map(x=>x.Date)
      .Not.Nullable()
      .Index("Ix_MetricDate")
      .UniqueKey("Key_Date_MetricType_Team");
    
    mapping.References(x=>x.MetricType)
      .Not.Nullable()
      .UniqueKey("Key_Date_MetricType_Team");
    
    mapping.References(x => x.Team)
      .Not.Nullable()
      .UniqueKey("Key_Date_MetricType_Team");

    mapping.HasManyToMany(x => x.Tickets)
      .Table("Ticket_Metric")
      .ParentKeyColumn("MetricId")
      .ChildKeyColumn("TicketId")
      .Cascade.SaveUpdate()
      .AsBag();

    mapping.HasManyToMany(x => x.Grades)
      .Table("Grades_Metric")
      .ParentKeyColumn("MetricId")
      .ChildKeyColumn("GradeId")
      .Cascade.SaveUpdate()
      .AsBag();
  }
}