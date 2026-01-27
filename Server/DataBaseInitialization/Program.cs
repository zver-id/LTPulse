using DBCore;
using NHibernate.Infrastructure;

namespace DataBaseInitialization;

class Program
{
  static void Main(string[] args)
  {
    var metrics = ExcelParser.ParseExcelToDictionaries("Aurora.xlsx", "tables");
    Console.WriteLine(metrics);
    var baseInitializer = new BaseTypeInitializer(
      new DbRepository(
        new NhibernateHelper("Host=localhost;Port=5432;Database=LTPulse;Username=admin;Password=Qwerty123")));
    baseInitializer.AddTeams();
    baseInitializer.AddMetricTypes();
    baseInitializer.AddPriorities();
    baseInitializer.AddTicketStates();
    baseInitializer.AddTeamMetricsFromExcel("Aurora.xlsx", "tables", "Аврора");
    baseInitializer.AddTeamMetricsFromExcel("atlas.xlsx", "tables", "Атлас");
  }
}