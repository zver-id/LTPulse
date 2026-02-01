using ClosedXML.Excel;
using CommonModels.Models;

namespace Application.ReportGeneration;

public class ReportWriter
{
  private ReportGenerator Generator { get; }

  public void CreateReport(Team team)
  {
    List<Metric> metrics = this.Generator.GetAllMetricsForTeam(team);
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

        tables.Cell(metric.MetricType.Id, 1).Value = metric.MetricType.Name;
      }
      dateNum++;
    }
    tables.Columns().AdjustToContents();
    workbook.SaveAs("report.xlsx");
  }

  public ReportWriter(ReportGenerator generator)
  {
    this.Generator = generator;
  }
}