using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

public class ExcelParser
{
  public static Dictionary<string, Dictionary<string, object>> ParseExcelToDictionaries(string filePath, string sheetName = null)
  {
    var result = new Dictionary<string, Dictionary<string, object>>();
    
    if (!File.Exists(filePath))
      throw new FileNotFoundException($"Файл не найден: {filePath}");
    
    using (var workbook = new XLWorkbook(filePath))
    {
      IXLWorksheet worksheet;
      if (string.IsNullOrEmpty(sheetName))
      {
        throw new ArgumentException($"Целевой лист не указан");
      }
      else
      {
        worksheet = workbook.Worksheets.Worksheet(sheetName);
        if (worksheet == null)
        {
          throw new ArgumentException($"Лист с именем '{sheetName}' не найден");
        }
      }

      var range = worksheet.RangeUsed();
      if (range == null)
      {
        throw new InvalidOperationException("Лист пуст");
      }
      
      int rowCount = range.RowCount();
      int colCount = range.ColumnCount();
      
      // Проверяем, что есть хотя бы одна строка данных
      if (rowCount < 2)
      {
        throw new InvalidOperationException("Недостаточно данных в листе");
      }
      
      // Получаем ключи из первого столбца (начиная со второй строки)
      var keys = new List<string>();
      for (int row = 2; row <= rowCount; row++)
      {
        var key = worksheet.Cell(row, 1).Value.ToString();
        if (!string.IsNullOrEmpty(key))
        {
          keys.Add(key);
        }
      }
      
      // Обрабатываем каждый столбец (начиная со второго)
      for (int col = 2; col <= colCount; col++)
      {
        // Получаем имя столбца из первой строки
        string columnName = worksheet.Cell(1, col).Value.ToString();
        if (string.IsNullOrEmpty(columnName))
        {
          columnName = $"Column_{col}";
        }
        
        var columnDict = new Dictionary<string, object>();
        
        // Заполняем словарь для текущего столбца
        for (int i = 0; i < keys.Count; i++)
        {
          int row = i + 2; // +2 потому что данные начинаются со второй строки
          if (row <= rowCount)
          {
            var cell = worksheet.Cell(row, col);
            object value = GetCellValue(cell);
            columnDict[keys[i]] = value;
          }
        }
        
        result[columnName] = columnDict;
      }
    }
    
    return result;
  }
  
  // Метод для получения значения ячейки с правильным типом
  private static object GetCellValue(IXLCell cell)
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
  
  // Альтернативная версия - возвращает список колонок как словарей
  public static List<ColumnDictionary> ParseExcelToColumnList(string filePath, string sheetName = null)
  {
    var result = new List<ColumnDictionary>();
    
    if (!File.Exists(filePath))
    {
      throw new FileNotFoundException($"Файл не найден: {filePath}");
    }
    
    using (var workbook = new XLWorkbook(filePath))
    {
      IXLWorksheet worksheet = string.IsNullOrEmpty(sheetName) 
        ? workbook.Worksheets.First() 
        : workbook.Worksheets.Worksheet(sheetName);
      
      if (worksheet == null)
      {
        throw new ArgumentException($"Лист не найден: {sheetName}");
      }
      
      var range = worksheet.RangeUsed();
      if (range == null)
      {
        throw new InvalidOperationException("Лист пуст");
      }
      
      int rowCount = range.RowCount();
      int colCount = range.ColumnCount();
      
      if (rowCount < 2)
      {
        throw new InvalidOperationException("Недостаточно данных в листе");
      }
      
      // Получаем ключи из первого столбца
      var keys = new List<string>();
      for (int row = 2; row <= rowCount; row++)
      {
        var key = worksheet.Cell(row, 1).Value.ToString();
        if (!string.IsNullOrEmpty(key))
        {
          keys.Add(key);
        }
      }
      
      // Создаем словарь для каждого столбца
      for (int col = 2; col <= colCount; col++)
      {
        var columnDict = new Dictionary<string, object>();
        string columnName = worksheet.Cell(1, col).Value.ToString();
        if (string.IsNullOrEmpty(columnName))
        {
          columnName = $"Column_{col}";
        }
        
        for (int i = 0; i < keys.Count; i++)
        {
          int row = i + 2;
          if (row <= rowCount)
          {
            var cell = worksheet.Cell(row, col);
            object value = GetCellValue(cell);
            columnDict[keys[i]] = value;
          }
        }
        
        result.Add(new ColumnDictionary
        {
          ColumnName = columnName,
          Data = columnDict
        });
      }
    }
    
    return result;
  }
  
  // Версия с обработкой разных типов данных
  public static Dictionary<string, Dictionary<string, T>> ParseExcelWithType<T>(string filePath, string sheetName = null) where T : IConvertible
  {
    var result = new Dictionary<string, Dictionary<string, T>>();
    
    using (var workbook = new XLWorkbook(filePath))
    {
      var worksheet = string.IsNullOrEmpty(sheetName) 
        ? workbook.Worksheets.First() 
        : workbook.Worksheets.Worksheet(sheetName);
      
      var range = worksheet.RangeUsed();
      int rowCount = range.RowCount();
      int colCount = range.ColumnCount();
      
      // Получаем ключи
      var keys = worksheet.Range(2, 1, rowCount, 1)
        .Cells()
        .Where(c => !string.IsNullOrEmpty(c.Value.ToString()))
        .Select(c => c.Value.ToString())
        .ToList();
      
      // Обрабатываем столбцы
      for (int col = 2; col <= colCount; col++)
      {
        string columnName = worksheet.Cell(1, col).Value.ToString() ?? $"Column_{col}";
        var columnDict = new Dictionary<string, T>();
        
        for (int i = 0; i < keys.Count; i++)
        {
          int row = i + 2;
          if (row <= rowCount)
          {
            var cell = worksheet.Cell(row, col);
            try
            {
              T value = (T)Convert.ChangeType(cell.Value, typeof(T));
              columnDict[keys[i]] = value;
            }
            catch
            {
              // В случае ошибки преобразования используем значение по умолчанию
              columnDict[keys[i]] = default(T);
            }
          }
        }
        
        result[columnName] = columnDict;
      }
    }
    
    return result;
  }
}

// Вспомогательный класс для структурированного возврата данных
public class ColumnDictionary
{
  public string ColumnName { get; set; }
  public Dictionary<string, object> Data { get; set; }
}