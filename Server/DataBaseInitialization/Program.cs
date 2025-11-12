using CommonModels.Models;
using DBCore;

namespace DataBaseInitialization;

class Program
{
  static void Main(string[] args)
  {
    var metrics = ExcelParser.ParseExcelToDictionaries("Aurora.xlsx", "tables");
    //Console.WriteLine(metrics);
    var baseInitializer = new BaseTypeInitializer(new DBRepository());
    baseInitializer.AddTeams();
    baseInitializer.AddMetricTypes();
    baseInitializer.AddTeamMetricsFromExcel("Aurora.xlsx", "tables", "Аврора");
  }
}