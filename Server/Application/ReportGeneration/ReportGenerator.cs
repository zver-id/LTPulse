using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using CommonModels.Attributes;
using CommonModels.Interfaces;
using CommonModels.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace Application.ReportGeneration;

/// <summary>
/// Генератор excel отчета.
/// </summary>
public class ReportGenerator
{
  #region Поля и свойства
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
  
  #endregion

  #region Методы

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
      // у графика по месяцам меняются месяцы, поэтому нужно именно новый
      if (metricGroup.Name == "Month")
      {
        this.GenerateChart(metricGroup, eChartType.Line);
      }
      else if (chartList.Drawings[metricGroup.NameOfChart] is ExcelChart chart)
      {
        this.RefreshChartSeries(chart, metricGroup);
      }
    }
    var allTicketsMetric = this.Repository.Get<Metric>(m => m.Team == team).First();
    this.AddListWithItems("Инциденты", allTicketsMetric);
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
    var startColumn = this.GetStartColumn(lastColumn);
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
  /// Сгенерировать график.
  /// </summary>
  /// <param name="metricGroup">Группа метрик.</param>
  /// <param name="chartType">Тип графика.</param>
  /// <exception cref="NullReferenceException"></exception>
  private void GenerateChart(MetricGroup metricGroup, eChartType chartType)
  {
    var chartWorksheet = this.ExcelPackage.Workbook.Worksheets["Графики"];
    var chart = chartWorksheet.Drawings.AddChart(metricGroup.NameOfChart, chartType);
    chart.Title.Text = metricGroup.NameOfChart; 
    chart.SetPosition(this.CurrentChartRow, 0, 1, 0);
    this.CurrentChartRow += ChartHeightInRows + 1;
    chart.SetSize(ChartWidthInPixel, ChartHeightInPixel);
    
    ExcelWorksheet? dataWorksheet = this.ExcelPackage.Workbook.Worksheets["tables"];
    if (dataWorksheet == null)
      throw new NullReferenceException("Изначально нужно заполнить лист с данными");
    var lastColumn = dataWorksheet.Dimension.End.Column;
    var startColumn = this.GetStartColumn(lastColumn);
    var seriesRowsNums = this.Repository.Get<MetricGroup>(mg => mg.Id == metricGroup.Id)
      .SelectMany(mg => mg.MetricTypes)
      .Select(m => m.Id);
    foreach (var seriesNum in seriesRowsNums)
    {
      var range = dataWorksheet.Cells[seriesNum, startColumn, seriesNum, lastColumn];
      if (range.All(cell => cell.Value == null))
        continue;
      this.AddSeriesToChart(chart, dataWorksheet, seriesNum, chartType);
    }
  }

  /// <summary>
  /// Добавить серию на график.
  /// </summary>
  /// <param name="chart">График.</param>
  /// <param name="dataWorksheet">Лист с данными.</param>
  /// <param name="rowNum">Номер строки с данными.</param>
  /// <param name="chartType">Тип графика.</param>
  private void AddSeriesToChart(ExcelChart chart, ExcelWorksheet dataWorksheet, int rowNum, eChartType chartType)
  {
    var lastColumn = dataWorksheet.Dimension.End.Column;
    var startColumn = this.GetStartColumn(lastColumn);
    switch (chartType)
    {
      case eChartType.Line:
        var seriesLine = chart.Series.Add(dataWorksheet.Cells[rowNum, startColumn, rowNum, lastColumn],
          dataWorksheet.Cells[1, startColumn, 1, lastColumn]) as ExcelLineChartSerie;
        seriesLine.Header = dataWorksheet.Cells[rowNum, 1].Value.ToString();
        seriesLine.DataLabel.ShowValue = true;
        seriesLine.DataLabel.Position = eLabelPosition.Top;
        break;
      case eChartType.Area:
        var seriesArea = chart.Series.Add(dataWorksheet.Cells[rowNum, startColumn, rowNum, lastColumn],
          dataWorksheet.Cells[1, startColumn, 1, lastColumn]) as ExcelAreaChartSerie;
        seriesArea.Header = dataWorksheet.Cells[rowNum, 1].Value.ToString();
        break;
      case eChartType.ColumnClustered:
        var seriesColumn = chart.Series.Add(dataWorksheet.Cells[rowNum, startColumn, rowNum, lastColumn],
          dataWorksheet.Cells[1, startColumn, 1, lastColumn]);
        break;
      default:
        return;
    }
  }

  /// <summary>
  /// Возвращает номер стартового столбца диапозона.
  /// </summary>
  /// <param name="lastColumn">Номер последнего столбца.</param>
  /// <returns>Номер первого столбца.</returns>
  private int GetStartColumn(int lastColumn)
  {
    var startColumn = lastColumn - DaysDataCountOnChart;
    if (startColumn < 2)
    {
      return 2;
    }
    return startColumn;
  }

  /// <summary>
  /// Добавить лист с элементами метрики.
  /// </summary>
  /// <param name="nameOfList">Наименование листа.</param>
  /// <param name="metric">Метрика из которой будем брать элементы.</param>
  private void AddListWithItems(string nameOfList, Metric metric)
  {
    var itemList = this.ExcelPackage.Workbook.Worksheets.Add(nameOfList);
    var propNames = DisplayNameGetter.GetPropertiesWithDisplayNames<Ticket>();
    for (var p = 0; p != propNames.Count; p++)
    {
      itemList.Cells[1, p + 1].Value = propNames.ElementAt(p).Key;
    }
    for (var ticket = 0; ticket != metric.Tickets.Count; ticket++)
    {
      for (var prop = 0; prop != propNames.Count; prop++)
      {
        itemList.Cells[ticket + 2, prop + 1].Value = propNames.
          ElementAt(prop).Value
          .GetValue(metric.Tickets[ticket])
          .ToString();
      }
    }
    this.FormatItemList(itemList);
  }

  /// <summary>
  /// Добавить форматирование на лист с данными.
  /// </summary>
  /// <param name="sheet">Лист с данными.</param>
  private void FormatItemList(ExcelWorksheet sheet)
  {
    sheet.Cells.AutoFitColumns(1, 50);
    
    var dataRange = sheet.Cells[sheet.Dimension.Address];
    dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
    dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
    dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
    dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
  }

  /// <summary>
  /// Получить шаблон отчета.
  /// </summary>
  /// <returns>Шаблон отчета в формате потока байт.</returns>
  private Stream? GetTemplate()
  {
    var assembly = Assembly.GetExecutingAssembly();
    string resourceName = "Application.ReportGeneration.report_template.xlsx";
    return assembly.GetManifestResourceStream(resourceName);
  }
  
  #endregion

  #region Конструкторы
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="repository">Репозиторий.</param>
  public ReportGenerator(IRepository repository)
  {
    this.Repository = repository; 
    this.ExcelPackage = new ExcelPackage(this.GetTemplate());
  }

  /// <summary>
  /// Статический конструктор.
  /// </summary>
  static ReportGenerator()
  {
    ExcelPackage.License.SetNonCommercialPersonal("LTPulse");
  }
  #endregion
}