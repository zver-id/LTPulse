using CommonModels.Interfaces;
using DBCore;
using TechKasConnector.DataCalculators;
using TechKasConnectService;

namespace TechKasConnector;

public static class Program
{
  public static void Main(string[] args)
  {
    var builder = Host.CreateApplicationBuilder(args);
    
    builder.Services.AddHostedService<MetricsCalculatorService>();
    
    builder.Services.AddScoped<IRepository, DbRepository>();
    builder.Services.AddScoped<MetricCalculator>();

    var host = builder.Build();
    host.Run();
  }
}