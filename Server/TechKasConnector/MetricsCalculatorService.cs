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
  private IHostApplicationLifetime ApplicationLifetime { get; init; }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using var scope = this.serviceScopeFactory.CreateScope();
    this.RabbitMqProducer = scope.ServiceProvider.GetRequiredService<RabbitMQClient>();
    var rabbitMqChanel = await this.RabbitMqProducer.Channel();
    this.Logger.LogInformation("MetricsCalculatorService running at: {time}", DateTimeOffset.Now);
    var rabbitMQConsumer = new AsyncEventingBasicConsumer(rabbitMqChanel);
    
    var maxConcurrentMessages  = Environment.ProcessorCount - 1;
    var messageSemaphore = new SemaphoreSlim(maxConcurrentMessages);
    
    await rabbitMqChanel.BasicQosAsync(
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
          this.ApplicationLifetime.StopApplication();
        }
        finally
        {
          messageSemaphore.Release();
        }
        //TODO нужно перенаправлять ошибочные сообщения в другую очередь
        await rabbitMqChanel.BasicAckAsync(ea.DeliveryTag, false);
      });
    };

    rabbitMqChanel.BasicConsumeAsync(
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
      
    IServiceScope scope = this.serviceScopeFactory.CreateScope();
    try
    {
      var metricCreator = scope.ServiceProvider.GetRequiredService<MetricCalculator>();
      await metricCreator.Init(messageBody.TeamId);
      await metricCreator.ProcessAllMetrics();
      return string.Empty;
    }
    catch (Exception ex)
    {
      this.Logger.LogError(ex, "Error processing message");
      throw;
    }
    finally
    {
      await Task.Delay(100);
      scope.Dispose();
    }
  }
  
  public MetricsCalculatorService(
    ILogger<MetricsCalculatorService> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration
    )
  {
    this.Logger = logger;
    this.serviceScopeFactory = serviceScopeFactory;
    this.Config = configuration;
  }
}