
using CommonModels.Interfaces;
using CommonModels.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace Application.ReportGeneration;

public class ReportGenerator
{
  /// <summary>
  /// Репозиторий.
  /// </summary>
  public IRepository Repository { get; }
  
  /// <summary>
  /// Команда.
  /// </summary>
  private Team Team { get; set; }
  
  /// <summary>
  /// Пакет отчета.
  /// </summary>
  private ExcelPackage ExcelPackage { get; } = new ();

  /// <summary>
  /// Сгенерировать отчёт по команде.
  /// </summary>
  /// <param name="team"></param>
  /// <returns></returns>
  public byte[] GenerateForTeam(Team team)
  {
    this.Team = team;
    this.FillMetricData();
    var mg = this.Repository.Get<MetricGroup>(mg => mg.Name == "Month").First();
    this.GenerateChart(mg);
    return this.ExcelPackage.GetAsByteArray();
  }

  /// <summary>
  /// Получить все метрики команды.
  /// </summary>
  /// <returns></returns>
  private List<Metric> GetAllMetricsForTeam()
  {
    return this.Repository.Get<Metric>(m => m.Team == this.Team);
  }

  /// <summary>
  /// Записать все метрики в отдельный лист.
  /// </summary>
  /// <param name="team"></param>
  /// <returns></returns>
  private void FillMetricData()
  {
    List<Metric> metrics = this.GetAllMetricsForTeam();
    var groupedMetrics = metrics.GroupBy(m => m.Date)
      .OrderBy(g => g.Key)
      .Select(group => new
      {
        Key = group.Key,
        Value = group.OrderBy(m => m.MetricType.Id)
      });
    
    var tables = this.ExcelPackage.Workbook.Worksheets.Add("tables");

    int dateNum = 2;
    foreach (var group in groupedMetrics)
    {
      tables.Cells[1, dateNum].Value = group.Key.ToString("dd.MM.yyyy");
      foreach (var metric in group.Value)
      {
        tables.Cells[metric.MetricType.Id, dateNum].Value = metric.Value;
        tables.Cells[metric.MetricType.Id, 1].Value = metric.MetricType.Name;
      }
      dateNum++;
    }
  }

  private void GenerateChart(MetricGroup metricGroup)
  {
    var chartWorksheet = this.ExcelPackage.Workbook.Worksheets.Add("Графики");
    var chart = chartWorksheet.Drawings.AddChart("Мк=есяца", eChartType.Line);
    chart.Title.Text = "Обращения по месяцам"; 
    chart.SetPosition(1, 0, 1, 0);
    chart.SetSize(1500, 400);
    
    var dataWorksheet = this.ExcelPackage.Workbook.Worksheets["tables"];
    if (dataWorksheet == null)
      throw new NullReferenceException("Изначально нужно заполнить лист с данными");
    var lastColumn = dataWorksheet.Dimension.End.Column;
    var seriesRowsNums = this.Repository.Get<MetricGroup>(mg => mg.Id == metricGroup.Id)
      .SelectMany(mg => mg.MetricTypes)
      .Select(m => m.Id);
    foreach (var seresNum in seriesRowsNums)
    {
      // пока что график на 70 дней
      var range = dataWorksheet.Cells[seresNum, lastColumn - 70, seresNum, lastColumn];
      if (range.All(cell => cell.Value == null))
        continue;
      var series = chart.Series.Add(dataWorksheet.Cells[seresNum, lastColumn - 70, seresNum, lastColumn],
        dataWorksheet.Cells[1, lastColumn-70, 1, lastColumn]) as ExcelLineChartSerie;
      series.Header = dataWorksheet.Cells[seresNum, 1].Value.ToString();
      series.DataLabel.ShowValue = true;
      series.DataLabel.Position = eLabelPosition.Top;
    }
  }

  #region Конструкторы
  public ReportGenerator(IRepository repository)
  {
    this.Repository = repository;
  }

  static ReportGenerator()
  {
    ExcelPackage.License.SetNonCommercialPersonal("LTPulse");
  }
  #endregion
}