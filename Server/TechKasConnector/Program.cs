using System.Configuration;
using CommonModels.Interfaces;
using DBCore;
using NHibernate.Infrastructure;
using TechKasConnector.Calendar;
using TechKasConnector.DataCalculators;
using TechKasConnectService;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

namespace TechKasConnector;

public static class Program
{
  public static void Main(string[] args)
  {
    var builder = Host.CreateApplicationBuilder(args);
    
    builder.Logging.ClearProviders();
    builder.Logging.AddNLog();
    
    var dataBaseConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    if (string.IsNullOrEmpty(dataBaseConnectionString))
      throw new ConfigurationErrorsException("PostgreSQL connection string not found");
    builder.Services.AddSingleton<NhibernateHelper>(service => new NhibernateHelper(dataBaseConnectionString));
    builder.Services.AddScoped<IRepository, DbRepository>();
    
    builder.Services.AddHostedService<SchedulerService>();
    builder.Services.AddHostedService<MetricsCalculatorService>();
    
    builder.Services.AddScoped<MetricCalculator>();
    builder.Services.AddScoped<GradeListGenerator>();
    builder.Services.AddScoped<JobScheduler>();
    builder.Services.AddScoped<CalendarCalculator>();
    builder.Services.Configure<ClubDetailsOptions>(
      builder.Configuration.GetSection("ClubDetails"));
    builder.Services.AddScoped<ExternalMessageCalculator>();

    var host = builder.Build();
    host.Run();
  }
}