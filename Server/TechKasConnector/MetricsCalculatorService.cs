using System.Text;
using System.Text.Json;
using Application;
using Application.RabbitMQRequests;
using CommonModels;
using CommonModels.Interfaces;
using CommonModels.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TechKasConnector.DataCalculators;

namespace TechKasConnectService;

/// <summary>
/// Сервис расчета метрик.
/// </summary>
public class MetricsCalculatorService : BackgroundService
{
  #region Поля и свойства
  /// <summary>
  /// Логгер.
  /// </summary>
  private ILogger<MetricsCalculatorService> Logger { get; set; }
  
  /// <summary>
  /// Точка доступа в RabbitMQ.
  /// </summary>
  private RabbitMQClient RabbitMqProducer { get; set; }
  
  /// <summary>
  /// Точка получения Scope.
  /// </summary>
  private IServiceScopeFactory serviceScopeFactory;
  
  #endregion

  
  /// <summary>
  /// Конфигурация.
  /// </summary>
  private IConfiguration Config { get; init; }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    this.Logger.LogInformation("MetricsCalculatorService running at: {time}", DateTimeOffset.Now);
    this.RabbitMqProducer = await RabbitMQClient.CreateAsync(this.Config.GetConnectionString("RabbitMQ"));
    var rabbitMQConsumer = new AsyncEventingBasicConsumer(this.RabbitMqProducer.channel);
    
    var maxConcurrentMessages  = Environment.ProcessorCount - 1;
    var messageSemaphore = new SemaphoreSlim(maxConcurrentMessages);
    
    await this.RabbitMqProducer.channel.BasicQosAsync(
      prefetchSize: 0, 
      prefetchCount: (ushort)maxConcurrentMessages, 
      global: false
    );
    
    rabbitMQConsumer.ReceivedAsync += async (ch, ea) =>
    {
      await messageSemaphore.WaitAsync(stoppingToken);
      _ = Task.Run(async () =>
      {
        try
        {
          var body = ea.Body.ToArray();
          var message = Encoding.UTF8.GetString(body);
          var correlationId = ea.BasicProperties.CorrelationId;
          var replyTo = ea.BasicProperties.ReplyTo;
          await this.ProcessMessage(message);
        }
        catch (Exception e)
        {
          this.Logger.LogError(e, "Error processing message");
          throw;
        }
        finally
        {
          messageSemaphore.Release();
        }
        //TODO нужно перенаправлять ошибочные сообщения в другую очередь
        await this.RabbitMqProducer.channel.BasicAckAsync(ea.DeliveryTag, false);
      });
    };

    this.RabbitMqProducer.channel.BasicConsumeAsync(
      queue: "task_queue",
      consumer: rabbitMQConsumer,
      autoAck: false
    );
  }

  private async Task<string> ProcessMessage(string message)
  {
    this.Logger.LogInformation("Processing message: {message}", message);
    var messageBody = JsonSerializer.Deserialize<GenerateTeamReportRequest>(message);
    if (messageBody == null)
    {
      this.Logger.LogError($"Received null message: {message}");
      throw new ArgumentException("Invalid message body");
    }
      
    using var scope = this.serviceScopeFactory.CreateScope();
    var metricCreator = scope.ServiceProvider.GetRequiredService<MetricCalculator>();
    metricCreator.Init(messageBody.Team);
    await metricCreator.ProcessAllMetrics();
    return string.Empty;
  }
  
  public MetricsCalculatorService(
    ILogger<MetricsCalculatorService> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration)
  {
    this.Logger = logger;
    this.serviceScopeFactory = serviceScopeFactory;
    this.Config = configuration;
  }
}