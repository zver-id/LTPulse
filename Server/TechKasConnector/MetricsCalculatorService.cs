using System.Text;
using Application;
using CommonModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TechKasConnectService;

public class MetricsCalculatorService : BackgroundService
{
  private readonly ILogger<MetricsCalculatorService> logger;
  private RabbitMQClient rabbitMQProducer;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("MetricsCalculatorService running at: {time}", DateTimeOffset.Now);
    this.rabbitMQProducer = await RabbitMQClient.CreateAsync(AppSettings.RabbitMQConnectionString);
    var rabbitMQConsumer = new AsyncEventingBasicConsumer(this.rabbitMQProducer.channel);
    rabbitMQConsumer.ReceivedAsync += async (ch, ea) =>
    {
      var body = ea.Body.ToArray();
      var message = Encoding.UTF8.GetString(body);
      var correlationId = ea.BasicProperties.CorrelationId;
      var replyTo = ea.BasicProperties.ReplyTo;
      Console.WriteLine($"Received message: {message}");
      await this.rabbitMQProducer.channel.BasicAckAsync(ea.DeliveryTag, false);
    };

    this.rabbitMQProducer.channel.BasicConsumeAsync(
      queue: "task_queue",
      consumer: rabbitMQConsumer,
      autoAck: false
    );
  }
  
  public MetricsCalculatorService(ILogger<MetricsCalculatorService> logger)
  {
    this.logger = logger;
  }
}