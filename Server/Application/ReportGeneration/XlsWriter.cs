using ClosedXML.Excel;
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
    using var workbook = new XLWorkbook();
    var tables = workbook.Worksheets.Add("tables");

    int dateNum = 2;
    foreach (var group in groupedMetrics)
    {
      tables.Cell(1, dateNum).Value = group.Key;
      foreach (var metric in group.Value)
      {
        tables.Cell(metric.MetricType.Id, dateNum).Value = metric.Value;
      }
      dateNum++;
    }
  }
}