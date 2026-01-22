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
    await this.SendMessage(message);
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
  /// <param name="connectionString">Строка подключения.</param>
  /// <returns>Экземпляр подключения.</returns>
  public static async Task<RabbitMQClient> CreateAsync(string connectionString)
  {
    var instance = new RabbitMQClient();
    var factory = new ConnectionFactory();
    factory.Uri = new Uri(connectionString);
    instance.connection = await factory.CreateConnectionAsync();
    instance.channel = await instance.connection.CreateChannelAsync();
    return instance;
  }
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  private RabbitMQClient() { }
}