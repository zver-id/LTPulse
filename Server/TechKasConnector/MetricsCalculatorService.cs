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

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    this.logger.LogInformation("MetricsCalculatorService running at: {time}", DateTimeOffset.Now);
    this.rabbitMQProducer = await RabbitMQClient.CreateAsync(AppSettings.RabbitMQConnectionString);
    var rabbitMQConsumer = new AsyncEventingBasicConsumer(this.rabbitMQProducer.channel);
    rabbitMQConsumer.ReceivedAsync += async (ch, ea) =>
    {
      var body = ea.Body.ToArray();
      var message = Encoding.UTF8.GetString(body);
      var correlationId = ea.BasicProperties.CorrelationId;
      var replyTo = ea.BasicProperties.ReplyTo;
      this.ProcessMessage(message);
      await this.rabbitMQProducer.channel.BasicAckAsync(ea.DeliveryTag, false);
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
    IServiceScopeFactory serviceScopeFactory)
  {
    this.logger = logger;
    this.serviceScopeFactory = serviceScopeFactory;
  }
}