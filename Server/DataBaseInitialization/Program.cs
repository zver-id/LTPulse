using CommonModels.Models;
using DBCore;

namespace XLStoDBConverter;

class Program
{
  static void Main(string[] args)
  {
    //var metrics = ExcelParser.ParseExcelToDictionaries("Aurora.xlsx", "tables");
    //Console.WriteLine(metrics);
    var baseInitializer = new BaseTypeInitializer(new DBRepository());
    baseInitializer.AddMetricTypes();
    baseInitializer.AddTeams();
  }
}