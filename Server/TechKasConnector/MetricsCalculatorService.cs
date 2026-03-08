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

public class MetricsCalculatorService : BackgroundService
{
  private readonly ILogger<MetricsCalculatorService> logger;
  private RabbitMQClient rabbitMQProducer;
  private IServiceScopeFactory serviceScopeFactory;
  
  /// <summary>
  /// Конфигурация.
  /// </summary>
  private IConfiguration Config { get; init; }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    this.logger.LogInformation("MetricsCalculatorService running at: {time}", DateTimeOffset.Now);
    this.rabbitMQProducer = await RabbitMQClient.CreateAsync(this.Config.GetConnectionString("RabbitMQ"));
    var rabbitMQConsumer = new AsyncEventingBasicConsumer(this.rabbitMQProducer.channel);
    
    var maxConcurrentMessages  = Environment.ProcessorCount - 1;
    var messageSemaphore = new SemaphoreSlim(maxConcurrentMessages);
    
    await this.rabbitMQProducer.channel.BasicQosAsync(
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
          this.ProcessMessage(message);
        }
        catch (Exception e)
        {
          this.logger.LogError(e, "Error processing message");
          throw;
        }
        finally
        {
          messageSemaphore.Release();
        }
        //TODO нужно перенаправлять ошибочные сообщения в другую очередь
        await this.rabbitMQProducer.channel.BasicAckAsync(ea.DeliveryTag, false);
      });
    };

    this.rabbitMQProducer.channel.BasicConsumeAsync(
      queue: "task_queue",
      consumer: rabbitMQConsumer,
      autoAck: false
    );
  }

  private string ProcessMessage(string message)
  {
    this.logger.LogInformation("Processing message: {message}", message);
    var messageBody = JsonSerializer.Deserialize<GenerateTeamReportRequest>(message);
    if (messageBody == null)
    {
      this.logger.LogError($"Received null message: {message}");
      throw new ArgumentException("Invalid message body");
    }
      
    using var scope = this.serviceScopeFactory.CreateScope();
    var metricCreator = scope.ServiceProvider.GetRequiredService<MetricCalculator>();
    metricCreator.Init(messageBody.Team);
    metricCreator.ProcessAllMetrics();
    return string.Empty;
  }
  
  public MetricsCalculatorService(ILogger<MetricsCalculatorService> logger,
    IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
  {
    this.logger = logger;
    this.serviceScopeFactory = serviceScopeFactory;
    this.Config = configuration;
  }
}