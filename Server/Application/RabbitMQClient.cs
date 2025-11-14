using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Application;

/// <summary>
/// Поставщик сообщений в RabbitMQ.
/// </summary>
public class RabbitMQClient
{
  /// <summary>
  /// Соединение.
  /// </summary>
  private IConnection connection { get; set;}
  
  /// <summary>
  /// Канал.
  /// </summary>
  private IChannel channel { get; set;}
  
  /// <summary>
  /// Очередь получения сообщений.
  /// </summary>
  private string requestQueueName = "task_queue";
  
  /// <summary>
  /// Очередь для отправки сообщений.
  /// </summary>
  private string responseQueueName = "response_queue";
  
  /// <summary>
  /// Хост для подключения.
  /// </summary>
  private string hostName;
  
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
    
    await channel.BasicPublishAsync(
      $"Ex",
      routingKey: this.requestQueueName,
      mandatory:false,
      basicProperties: new BasicProperties(),
      body: body);
  }
  
  /// <summary>
  /// Создать экземпляр. 
  /// </summary>
  /// <param name="hostName">Хост подключения.</param>
  /// <returns>Экземпляр подключения.</returns>
  public static async Task<RabbitMQClient> CreateAsync(string hostName)
  {
    var instance = new RabbitMQClient();
    instance.hostName = hostName;
    var factory = new ConnectionFactory() { HostName = hostName };;
    instance.connection = await factory.CreateConnectionAsync();
    instance.channel = await instance.connection.CreateChannelAsync();
    return instance;
  }
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  private RabbitMQClient() { }
}