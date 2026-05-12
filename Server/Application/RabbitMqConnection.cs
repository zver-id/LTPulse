using RabbitMQ.Client;

namespace Application;

/// <summary>
/// Коммуникатор.
/// </summary>
public class RabbitMqConnection : IDisposable
{
  /// <summary>
  /// Соединение.
  /// </summary>
  public IConnection Connection { get; set;}
  
  /// <summary>
  /// Создать экземпляр. 
  /// </summary>
  /// <param name="connectionString">Строка подключения.</param>
  /// <returns>Экземпляр соединения.</returns>
  public static async Task<RabbitMqConnection> CreateAsync(string connectionString)
  {
    var instance = new RabbitMqConnection();
    var factory = new ConnectionFactory
    {
      Uri = new Uri(connectionString)
    };
    instance.Connection = await factory.CreateConnectionAsync();
    return instance;
  }
  
  private RabbitMqConnection(){}

  public void Dispose()
  {
    this.Connection.Dispose();
  }
}