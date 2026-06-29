using RabbitMQ.Client;

namespace Application;

public class RabbitMQConsumer
{
  /// <summary>
  /// Соединение.
  /// </summary>
  private IConnection Connection { get; set;}
  
  /// <summary>
  /// Имя очереди подписки.
  /// </summary>
  private string QueueName { get; set;}

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="connection">Соединение.</param>
  /// <param name="queueName">Имя очереди подписки.</param>
  public RabbitMQConsumer(RabbitMqConnection connection, string queueName)
  {
    this.Connection = connection.Connection;
    this.QueueName = queueName;
  }
}