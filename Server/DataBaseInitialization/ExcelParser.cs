using ClosedXML.Excel;

namespace DataBaseInitialization;

/// <summary>
/// Загрузка данных из существующих файлов.
/// </summary>
public class ExcelParser
{
  /// <summary>
  /// Получить данные из существующего excel файла.
  /// </summary>
  /// <param name="filePath">Путь до файла.</param>
  /// <param name="sheetName">Имя листа с данными.</param>
  /// <returns>Словарь с данными в формате {дата: {имя метрики: значение метрики}}</returns>
  /// <exception cref="FileNotFoundException">Ошибка отсутствующего файла.</exception>
  /// <exception cref="ArgumentException">Возникает при неверном указании листа</exception>
  /// <exception cref="InvalidOperationException">Возникает, если данных в листе нет.</exception>
  public static Dictionary<string, Dictionary<string, float>> ParseExcelToDictionaries(string filePath,
    string sheetName = "tables")
  {
    var result = new Dictionary<string, Dictionary<string, float>>();

    if (!File.Exists(filePath))
      throw new FileNotFoundException($"Файл не найден: {filePath}");

    using (var workbook = new XLWorkbook(filePath))
    {
      IXLWorksheet worksheet;
      if (string.IsNullOrEmpty(sheetName))
        throw new ArgumentException($"Целевой лист не указан");

      worksheet = workbook.Worksheets.Worksheet(sheetName);
      if (worksheet == null)
        throw new ArgumentException($"Лист с именем '{sheetName}' не найден");

      var range = worksheet.RangeUsed();
      if (range == null)
        throw new InvalidOperationException("Лист пуст");

      int rowCount = range.RowCount();
      int colCount = range.ColumnCount();

      // Обрабатываем каждый столбец (начиная со второго)
      for (int col = 2; col <= colCount; col++)
      {
        // Получаем имя столбца из первой строки
        string columnName = worksheet.Cell(1, col).Value.ToString();
        if (string.IsNullOrEmpty(columnName))
          throw new ArgumentException("Пропущено значение даты");

        var columnDict = new Dictionary<string, float>();

        // Заполняем словарь для текущего столбца
        for (int row = 2; row <= rowCount; row++) // +2 потому что данные начинаются со второй строки
        {
          var cell = worksheet.Cell(row, col);
          float.TryParse(
            GetCellValue(cell).ToString(),
            out var value);
          var key = worksheet.Cell(row, 1).Value.ToString();
          if (string.IsNullOrEmpty(key))
            continue;
          columnDict[key] = value;
        }

        result[columnName] = columnDict;
      }
    }

    return result;
  }

  /// <summary>
  /// Получить значение ячейки с правильным типом
  /// </summary>
  /// <param name="cell"></param>
  /// <returns>Значение ячейки</returns>
  private static object? GetCellValue(IXLCell cell)
  {
    return cell.Value.Type switch
    {
      XLDataType.Text => cell.Value.GetText(),
      XLDataType.Number => cell.Value.GetNumber(),
      XLDataType.Boolean => cell.Value.GetBoolean(),
      XLDataType.DateTime => cell.Value.GetDateTime(),
      XLDataType.TimeSpan => cell.Value.GetTimeSpan(),
      XLDataType.Blank => null,
      _ => cell.Value.ToString()
    };
  }
}