using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
  private IConnection Connection { get; set;}
  
  /// <summary>
  /// Канал.
  /// </summary>
  //public IChannel Channel { get; private set;}
  
  /// <summary>
  /// Канал.
  /// </summary>
  private readonly Lazy<Task<IChannel>> lazyChannel;
  public async Task<IChannel> Channel() => await this.lazyChannel.Value;
  
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
    var channel = await this.Channel();
    await channel.QueueDeclareAsync(queue: this.requestQueueName,
      durable: false,
      exclusive: false,
      autoDelete: false,
      arguments: null);

    var body = Encoding.UTF8.GetBytes(message);
    
    await channel.BasicPublishAsync(
      String.Empty, 
      routingKey: this.requestQueueName,
      mandatory:false,
      basicProperties: new BasicProperties(),
      body: body);
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  public RabbitMQClient(RabbitMqConnection connection)
  {
    this.Connection = connection.Connection;
    this.lazyChannel = new Lazy<Task<IChannel>>(() => this.Connection.CreateChannelAsync());
  }
}