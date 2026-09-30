using System.Configuration;
using Application;
using CommonModels.Interfaces;
using DBCore;
using NHibernate.Infrastructure;
using TechKasConnector.Calendar;
using TechKasConnector.DataCalculators;
using TechKasConnector.Mattermost;
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

    builder.Services.AddWindowsService(options =>
    {
      options.ServiceName = "TechKasConnector";
    });
    
    var dataBaseConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    if (string.IsNullOrEmpty(dataBaseConnectionString))
      throw new ConfigurationErrorsException("PostgreSQL connection string not found");
    builder.Services.AddSingleton<NhibernateHelper>(service => new NhibernateHelper(dataBaseConnectionString));
    builder.Services.AddSingleton<RabbitMqConnection>(service =>
    {
      var rabbitMQconnectionString = builder.Configuration.GetConnectionString("RabbitMQ");
      if (string.IsNullOrEmpty(rabbitMQconnectionString))
        throw new ConfigurationErrorsException("RabbitMQ connection string not found");
      return RabbitMqConnection.CreateAsync(rabbitMQconnectionString).GetAwaiter().GetResult();
    });
    
    builder.Services.AddScoped<IUnitOfWork>(sp =>
        new UnitOfWork(sp.GetRequiredService<NhibernateHelper>().SessionFactory));
    builder.Services.AddScoped<IRepository>(sp =>
        new DbRepository(sp.GetRequiredService<IUnitOfWork>()));
    
    builder.Services.AddHostedService<SchedulerService>();
    builder.Services.AddHostedService<MetricsCalculatorService>();

    builder.Services.AddScoped<RabbitMQClient>();
    builder.Services.AddScoped<MetricCalculator>();
    builder.Services.AddScoped<GradeListGenerator>();
    builder.Services.AddScoped<JobScheduler>();
    builder.Services.AddScoped<CalendarCalculator>();
    builder.Services.AddScoped<TechKasConnector.Queries.JobCalculatingQueryRepository>(
      sp => new TechKasConnector.Queries.JobCalculatingQueryRepository(sp.GetRequiredService<IUnitOfWork>()));
    builder.Services.Configure<ClubDetailsOptions>(
      builder.Configuration.GetSection("ClubDetails"));
    builder.Services.AddScoped<ExternalMessageCalculator>();
    builder.Services.Configure<MattermostOptions>(
      builder.Configuration.GetSection("Mattermost"));
    builder.Services.AddScoped<MattermostClient>();

    var host = builder.Build();
    host.Run();
  }
}