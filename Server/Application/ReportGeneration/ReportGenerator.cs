
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
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
  private ExcelPackage ExcelPackage { get; }

  /// <summary>
  /// Текущая строка графика.
  /// </summary>
  private int CurrentChartRow { get; set; } = 1;

  /// <summary>
  /// Высота графика в ячеках.
  /// </summary>
  private const int ChartHeightInRows = 30;
  
  /// <summary>
  /// Высота графика в пикселях.
  /// </summary>
  private const int ChartHeightInPixel = ChartHeightInRows * 20;
  
  /// <summary>
  /// Ширина графика в пикселях.
  /// </summary>
  private const int ChartWidthInPixel = 1500;
  
  /// <summary>
  /// Количество данных по дням на графике.
  /// </summary>
  private const int DaysDataCountOnChart = 70;

  /// <summary>
  /// Сгенерировать отчёт по команде.
  /// </summary>
  /// <param name="team"></param>
  /// <returns></returns>
  public byte[] GenerateForTeam(Team team)
  {
    this.Team = team;
    var chartList = this.ExcelPackage.Workbook.Worksheets["Графики"];
    this.FillMetricData();
    var metricGroups = this.Repository.Get<MetricGroup>(mg => true);
    var count = 0;
    foreach (var metricGroup in metricGroups)
    {
      if (chartList.Drawings[metricGroup.NameOfChart] is ExcelChart chart)
      {
        this.RefreshChartSeries(chart, metricGroup);
      }
      /*
      switch (metricGroup.ChartType)
      {
        case "Line":
          this.GenerateLineChart(metricGroup);
          break;
        case "Area":
          this.GenerateAreaChart(metricGroup);
          break;
        case "Column":
          this.GenerateColumnChart(metricGroup);
          break;
        default:
          continue;
      }
      */
    }
    
    
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

  /// <summary>
  /// Обновить данные серий в графике.
  /// </summary>
  /// <param name="chart">График.</param>
  /// <param name="metricGroup">Группа метрик.</param>
  private void RefreshChartSeries(ExcelChart chart, MetricGroup metricGroup)
  {
    var dataWorksheet = this.ExcelPackage.Workbook.Worksheets["tables"];
    var lastColumn = dataWorksheet.Dimension.End.Column;
    var series = chart.Series;
    var metrics = this.Repository.Get<MetricGroup>(mg => mg.Id == metricGroup.Id)
      .SelectMany(mg => mg.MetricTypes);
    var startColumn = lastColumn - DaysDataCountOnChart;
    if (startColumn < 2)
    {
      startColumn = 2;
    }
    foreach (var metric in metrics)
    {
      var serie = series.FirstOrDefault(s => s.Header == metric.Name);
      if (serie != null)
      {
        serie.Series = dataWorksheet.Cells[metric.Id, startColumn, metric.Id, lastColumn].FullAddress;
        serie.XSeries = dataWorksheet.Cells[1, startColumn, 1, lastColumn].FullAddress;
      }
    }
  }

  /// <summary>
  /// Сгенерировать линейный график.
  /// </summary>
  /// <param name="metricGroup">Группа метрик.</param>
  /// <exception cref="NullReferenceException"></exception>
  private void GenerateLineChart(MetricGroup metricGroup)
  {
    var chartWorksheet = this.ExcelPackage.Workbook.Worksheets["Графики"];
    var chart = chartWorksheet.Drawings.AddChart(metricGroup.NameOfChart, eChartType.Line);
    chart.Title.Text = metricGroup.NameOfChart; 
    chart.SetPosition(this.CurrentChartRow, 0, 1, 0);
    this.CurrentChartRow += ChartHeightInRows + 1;
    chart.SetSize(ChartWidthInPixel, ChartHeightInPixel);
    
    var dataWorksheet = this.ExcelPackage.Workbook.Worksheets["tables"];
    if (dataWorksheet == null)
      throw new NullReferenceException("Изначально нужно заполнить лист с данными");
    var lastColumn = dataWorksheet.Dimension.End.Column;
    var seriesRowsNums = this.Repository.Get<MetricGroup>(mg => mg.Id == metricGroup.Id)
      .SelectMany(mg => mg.MetricTypes)
      .Select(m => m.Id);
    foreach (var seresNum in seriesRowsNums)
    {
      var range = dataWorksheet.Cells[seresNum, lastColumn - DaysDataCountOnChart, seresNum, lastColumn];
      if (range.All(cell => cell.Value == null))
        continue;
      var series = chart.Series.Add(dataWorksheet.Cells[seresNum, lastColumn - DaysDataCountOnChart, seresNum, lastColumn],
        dataWorksheet.Cells[1, lastColumn-DaysDataCountOnChart, 1, lastColumn]) as ExcelLineChartSerie;
      series.Header = dataWorksheet.Cells[seresNum, 1].Value.ToString();
      series.DataLabel.ShowValue = true;
      series.DataLabel.Position = eLabelPosition.Top;
    }
  }
  
  private void GenerateAreaChart(MetricGroup metricGroup)
  {
    var chartWorksheet = this.ExcelPackage.Workbook.Worksheets["Графики"];
    var chart = chartWorksheet.Drawings.AddChart(metricGroup.NameOfChart, eChartType.AreaStacked100);
    chart.Title.Text = metricGroup.NameOfChart;
    chart.Legend.Position = eLegendPosition.Right;
    chart.SetPosition(this.CurrentChartRow, 0, 1, 0);
    this.CurrentChartRow += ChartHeightInRows + 1;
    chart.SetSize(ChartWidthInPixel, ChartHeightInPixel);
    
    var dataWorksheet = this.ExcelPackage.Workbook.Worksheets["tables"];
    if (dataWorksheet == null)
      throw new NullReferenceException("Изначально нужно заполнить лист с данными");
    var lastColumn = dataWorksheet.Dimension.End.Column;
    var metricTypes = this.Repository.Get<MetricGroup>(mg => mg.Id == metricGroup.Id)
      .SelectMany(mg => mg.MetricTypes);
    foreach (var metricType in metricTypes)
    {
      var range = dataWorksheet.Cells[metricType.Id, lastColumn - DaysDataCountOnChart, metricType.Id, lastColumn];
      if (range.All(cell => cell.Value == null))
        continue;
      var series = chart.Series.Add(dataWorksheet.Cells[metricType.Id, lastColumn - DaysDataCountOnChart, metricType.Id, lastColumn],
        dataWorksheet.Cells[1, lastColumn-DaysDataCountOnChart, 1, lastColumn]) as ExcelAreaChartSerie;

      if (series != null)
      {
        series.Header = metricType.Name;
        //series.DataLabel.ShowValue = true;
        //series.DataLabel.Format = "0";
      }
    }
  }
  
  private void GenerateColumnChart(MetricGroup metricGroup)
  {
    var chartWorksheet = this.ExcelPackage.Workbook.Worksheets["Графики"];
    var chart = chartWorksheet.Drawings.AddChart(metricGroup.NameOfChart, eChartType.ColumnClustered);
    chart.Title.Text = metricGroup.NameOfChart; 
    chart.SetPosition(this.CurrentChartRow, 0, 1, 0);
    this.CurrentChartRow += ChartHeightInRows + 1;
    chart.SetSize(ChartWidthInPixel, ChartHeightInPixel);
    
    var dataWorksheet = this.ExcelPackage.Workbook.Worksheets["tables"];
    if (dataWorksheet == null)
      throw new NullReferenceException("Изначально нужно заполнить лист с данными");
    var lastColumn = dataWorksheet.Dimension.End.Column;
    var seriesRowsNums = this.Repository.Get<MetricGroup>(mg => mg.Id == metricGroup.Id)
      .SelectMany(mg => mg.MetricTypes)
      .Select(m => m.Id);
    foreach (var seresNum in seriesRowsNums)
    {
      var range = dataWorksheet.Cells[seresNum, lastColumn - DaysDataCountOnChart, seresNum, lastColumn];
      if (range.All(cell => cell.Value == null))
        continue;
      var series = chart.Series.Add(dataWorksheet.Cells[seresNum, lastColumn - DaysDataCountOnChart, seresNum, lastColumn],
        dataWorksheet.Cells[1, lastColumn-DaysDataCountOnChart, 1, lastColumn]);
      series.Header = dataWorksheet.Cells[seresNum, 1].Value.ToString();
      //series.DataLabel.ShowValue = true;
      //series.DataLabel.Position = eLabelPosition.Top;
    }
  }

  private Stream? GetTemplate()
  {
    var assembly = Assembly.GetExecutingAssembly();
    string resourceName = "Application.ReportGeneration.report_template.xlsx";
    return assembly.GetManifestResourceStream(resourceName);
  }

  #region Конструкторы
  
  public ReportGenerator(IRepository repository)
  {
    this.Repository = repository; 
    this.ExcelPackage = new ExcelPackage(this.GetTemplate());
    //this.ExcelPackage = new ExcelPackage();
  }

  static ReportGenerator()
  {
    ExcelPackage.License.SetNonCommercialPersonal("LTPulse");
    //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
  }
  #endregion
}