using System.Configuration;
using CommonModels.Interfaces;
using DBCore;
using NHibernate.Infrastructure;
using TechKasConnector.Calendar;
using TechKasConnector.DataCalculators;
using TechKasConnectService;

namespace TechKasConnector;

public static class Program
{
  public static void Main(string[] args)
  {
    var builder = Host.CreateApplicationBuilder(args);
    
    var dataBaseConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    if (string.IsNullOrEmpty(dataBaseConnectionString))
      throw new ConfigurationErrorsException("PostgreSQL connection string not found");
    builder.Services.AddSingleton<NhibernateHelper>(service => new NhibernateHelper(dataBaseConnectionString));
    builder.Services.AddScoped<IRepository, DbRepository>();
    
    builder.Services.AddHostedService<MetricsCalculatorService>();
    builder.Services.AddScoped<MetricCalculator>();
    builder.Services.AddScoped<CalendarCalculator>();

    var host = builder.Build();
    host.Run();
  }
}