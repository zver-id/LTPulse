using CommonModels.Models;

namespace Application.ReportGeneration;

public class XlsWriter
{
  private ReportGenerator Generator { get; }

  private void CreateReport()
  {
    List<Metric> metrics = this.Generator.GetAllMetrics();
    var groupedMetrics = metrics.GroupBy(m => m.Date)
      .OrderBy(g => g.Key)
      .Select(group => new
      {
        Key = group.Key,
        Value = group.OrderBy(m => m.MetricType.Id)
      });
    
  }
}