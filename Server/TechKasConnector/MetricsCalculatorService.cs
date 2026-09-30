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
using TechKasConnector.Queries;

namespace TechKasConnectService;

/// <summary>
/// Специальные результаты обработки сообщения.
/// </summary>
internal static class SpecialResults
{
  /// <summary>
  /// Сообщение нужно отбросить (расчет метрик команды уже выполняется).
  /// </summary>
  internal const string DropMessage = "__drop_message__";
}

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
    // При старте сбрасываем признак "рассчитываются" у всех задач: после аварийного
    // завершения какое-то значение могло остаться true и тогда все сообщения
    // были бы отброшены.
    await this.ResetCalculatingFlags();

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
    
    var errorTcs = new TaskCompletionSource<bool>();
    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
    
    rabbitMQConsumer.ReceivedAsync += async (ch, ea) =>
    {
      _ = Task.Run(async () =>
      {
        await messageSemaphore.WaitAsync(stoppingToken);
        try
        {
          var body = ea.Body.ToArray();
          var message = Encoding.UTF8.GetString(body);
          var result = await this.ProcessMessage(message);
          if (result == SpecialResults.DropMessage)
          {
            // Расчет уже идет: сообщение отбрасываем (без повторной доставки).
            await rabbitMqChanel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
          }
          else
          {
            await rabbitMqChanel.BasicAckAsync(ea.DeliveryTag, false);
          }
        }
        catch (Exception e)
        {
          this.Logger.LogError(e, "Error processing message. The application has stopped.");
          linkedCts.Cancel();
          errorTcs.SetException(e);
          throw;
        }
        finally
        {
          messageSemaphore.Release();
        }
      });
    };

    rabbitMqChanel.BasicConsumeAsync(
      queue: "task_queue",
      consumer: rabbitMQConsumer,
      autoAck: false
    );
    
    try
    {
      await Task.WhenAny(
        Task.Delay(Timeout.Infinite, linkedCts.Token),
        errorTcs.Task
      );
      await errorTcs.Task;
      this.Logger.LogCritical("Critical error occurred in message processing. Stopping application.");
    }
    catch (Exception ex)
    {
      this.Logger.LogCritical(ex, "Critical error occurred in message processing. Stopping application.");
      Environment.Exit(1);
    }
    this.Logger.LogDebug("Метод Execute Async завершился в MetricsCalculatorService");
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

    var scope = this.serviceScopeFactory.CreateAsyncScope();
    try
    {
      var queryRepository = scope.ServiceProvider.GetRequiredService<JobCalculatingQueryRepository>();

      // Атомарно "занимаем" задачу: true устанавливается только если сейчас false.
      // Если кто-то уже считает метрики этой команды, сообщение отбрасываем (nack).
      var claimed = await queryRepository.ClaimJobAsync(messageBody.TeamId);
      if (!claimed)
      {
        this.Logger.LogInformation(
          "Метрики команды {teamId} уже рассчитываются, сообщение отброшено.",
          messageBody.TeamId);
        return SpecialResults.DropMessage;
      }

      try
      {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var metricCreator = scope.ServiceProvider.GetRequiredService<MetricCalculator>();
        try
        {
          await metricCreator.Init(messageBody.TeamId);
        }
        catch (Exception initEx)
        {
          // Ошибки загрузки (например, обращение с полем длиннее колонки)
          // не должны останавливать весь сбор: логируем и продолжаем.
          this.Logger.LogError(initEx, "Ошибка при инициализации калькулятора для команды {teamId}", messageBody.TeamId);
        }
        await metricCreator.ProcessAllMetrics();
        // Метрики по сотрудникам не используются во фронтенде (эндпоинт
        // /api/Metrics/employee без вызовов), отключено для сокращения нагрузки.
        //await metricCreator.ProcessEmployeeMetrics();

        var team = await repository.GetById<Team>(messageBody.TeamId);
        team.LastMetricsCalculated = DateTime.Now;
        await repository.Update(team);

        return string.Empty;
      }
      finally
      {
        // Независимо от успеха/ошибки снимаем признак, чтобы расчет мог запуститься снова.
        await queryRepository.ResetJobAsync(messageBody.TeamId);
      }
    }
    catch (Exception ex)
    {
      this.Logger.LogError(ex, "Error processing message.");
      throw;
    }
    finally
    {
      await Task.Delay(100);
      await scope.DisposeAsync();
    }
  }

  /// <summary>
  /// Сбросить признак "рассчитываются" у всех задач (вызывается при старте сервиса).
  /// </summary>
  private async Task ResetCalculatingFlags()
  {
    try
    {
      using var scope = this.serviceScopeFactory.CreateScope();
      var queryRepository = scope.ServiceProvider.GetRequiredService<JobCalculatingQueryRepository>();
      await queryRepository.ResetAllAsync();
      this.Logger.LogInformation("Сброшены все флаги расчета метрик при старте.");
    }
    catch (Exception ex)
    {
      this.Logger.LogWarning(ex, "Не удалось сбросить флаги расчета при старте.");
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