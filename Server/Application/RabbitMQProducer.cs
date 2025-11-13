using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Application;

/// <summary>
/// Поставщик сообщений в RabbitMQ.
/// </summary>
public class RabbitMQProducer
{
  /// <summary>
  /// Соединение.
  /// </summary>
  private IConnection connection { get; set;}
  
  /// <summary>
  /// Канал.
  /// </summary>
  public IChannel channel { get; private set;}
  
  /// <summary>
  /// Очередь получения сообщений.
  /// </summary>
  private string requestQueueName = "task_queue";
  
  /// <summary>
  /// Очередь для отправки сообщений.
  /// </summary>
  private string responseQueueName = "response_queue";
  
  /// <summary>
  /// Отправить сообщение.
  /// </summary>
  /// <param name="obj">Объект для отправки.</param>
  public async Task SendMessage(object obj)
  {
    var message = JsonSerializer.Serialize(obj);
    await SendMessage(message);
  }

  /// <summary>
  /// Отправить строку в сообщении RabbitMQ.
  /// </summary>
  /// <param name="message">Строка для отправки.</param>
  public async Task SendMessage(string message)
  {
    await channel.QueueDeclareAsync(queue: this.requestQueueName,
      durable: false,
      exclusive: false,
      autoDelete: false,
      arguments: null);

    var body = Encoding.UTF8.GetBytes(message);
    var publishProperties = new BasicProperties();
    publishProperties.Persistent = true;
    await channel.BasicPublishAsync(
      $"",
      routingKey: this.requestQueueName,
      mandatory:true,
      basicProperties: publishProperties,
      body: body);
  }

  /// <summary>
  /// Создать экземпляр. 
  /// </summary>
  /// <param name="uriString">Строка подключения.</param>
  /// <returns>Экземпляр подключения.</returns>
  public static async Task<RabbitMQProducer> CreateAsync
    (string uriString)
  {
    var instance = new RabbitMQProducer();
    var factory = new ConnectionFactory();
    factory.Uri = new Uri(uriString);
    instance.connection = await factory.CreateConnectionAsync();
    instance.channel = await instance.connection.CreateChannelAsync();
    return instance;
  }
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  private RabbitMQProducer() { }
}