using System;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;

namespace DBCore;

/// <summary>
/// Единица работы (Unit of Work): владелец одной сессии NHibernate
/// на область жизни (веб-запрос, сообщение, тест). Все репозитории в одном
/// scope используют одну и ту же сессию, поэтому прочитанные сущности
/// остаются managed и их можно сохранять без конфликтов по первичному ключу.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
  /// <summary>
  /// Текущая сессия (создается лениво при первом обращении).
  /// </summary>
  ISession Session { get; }

  /// <summary>
  /// Выполнить операцию, требующую прямой доступа к сессии (Get, Query, Save,
  /// Update, Delete, Flush и т.п.), с гарантией, что на единственной сессии
  /// области в данный момент выполняется не более одной операции с БД.
  /// </summary>
  /// <param name="operation">Операция над сессией.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <typeparam name="T">Тип возвращаемого значения.</typeparam>
  /// <returns>Результат операции.</returns>
  Task<T> ExecuteOnSessionAsync<T>(Func<ISession, Task<T>> operation,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Выполнить операцию, не возвращающую значение, с гарантией отсутствия
  /// наложения операций на единственной сессии области.
  /// </summary>
  /// <param name="operation">Операция над сессией.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  Task ExecuteOnSessionAsync(Func<ISession, Task> operation,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Выполнить синхронную операцию, требующую прямого доступа к сессии, с
  /// гарантией, что на единственной сессии области в данный момент выполняется
  /// не более одной операции с БД.
  /// </summary>
  /// <param name="operation">Операция над сессией.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <typeparam name="T">Тип возвращаемого значения.</typeparam>
  /// <returns>Результат операции.</returns>
  T ExecuteOnSession<T>(Func<ISession, T> operation,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Получить сессию области для выполнения низкоуровневых операций (например,
  /// нативных SQL-команд), которые нельзя выразить методами репозитория.
  /// </summary>
  ISession GetSession();

  /// <summary>
  /// Зафиксировать изменения и завершить транзакцию.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены.</param>
  Task CommitAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Откатить текущую транзакцию.
  /// </summary>
  /// <param name="cancellationToken">Токен отмены.</param>
  Task RollbackAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Реализация <see cref="IUnitOfWork"/>: одна лениво создаваемая сессия
/// и одна лениво создаваемая транзакция до завершения области жизни.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
  private readonly ISessionFactory sessionFactory;
  private ISession? session;
  private ITransaction? transaction;

  /// <summary>
  /// Замок, исключающий одновременное выполнение двух операций на одной сессии.
  /// Один scope (= одна сессия/транзакция) может использоваться из нескольких
  /// потоков (параллельная обработка сообщений, планировщик), а подключение
  /// Npgsql не допускает наложения операций, поэтому все обращения к сессии
  /// сериализуются этим замком.
  /// </summary>
  private readonly AsyncLock dbLock = new();

  /// <summary>
  /// Текущая сессия (создается лениво при первом обращении).
  /// </summary>
  public ISession Session => this.session ??= this.sessionFactory.OpenSession();

  private ITransaction Transaction => this.transaction ??= this.Session.BeginTransaction();

  /// <summary>
  /// Выполнить операцию, требующую прямого доступа к сессии, с гарантией
  /// отсутствия наложения операций на единственной сессии области.
  /// </summary>
  public async Task<T> ExecuteOnSessionAsync<T>(Func<ISession, Task<T>> operation,
    CancellationToken cancellationToken = default)
  {
    return await this.dbLock.WithLockAsync(
      async () => await operation(this.Session).ConfigureAwait(false),
      cancellationToken);
  }

  /// <summary>
  /// Выполнить операцию, не возвращающую значение, с гарантией отсутствия
  /// наложения операций на единственной сессии области.
  /// </summary>
  public async Task ExecuteOnSessionAsync(Func<ISession, Task> operation,
    CancellationToken cancellationToken = default)
  {
    await this.dbLock.WithLockAsync(
      async () => await operation(this.Session).ConfigureAwait(false),
      cancellationToken);
  }

  /// <summary>
  /// Выполнить синхронную операцию, требующую прямого доступа к сессии, с
  /// гарантией отсутствия наложения операций на единственной сессии области.
  /// </summary>
  public T ExecuteOnSession<T>(Func<ISession, T> operation,
    CancellationToken cancellationToken = default)
  {
    return this.dbLock.WithLock(() => operation(this.Session), cancellationToken);
  }

  /// <summary>
  /// Получить сессию области для выполнения низкоуровневых операций.
  /// </summary>
  public ISession GetSession()
  {
    return this.Session;
  }

  /// <summary>
  /// Зафиксировать изменения и завершить транзакцию.
  /// </summary>
  public async Task CommitAsync(CancellationToken cancellationToken = default)
  {
    await this.dbLock.WithLockAsync(async () =>
    {
      if (this.transaction?.IsActive == true)
      {
        await this.transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
      }
      else
      {
        await this.Session.FlushAsync(cancellationToken).ConfigureAwait(false);
      }
    }, cancellationToken);
  }

  /// <summary>
  /// Откатить текущую транзакцию.
  /// </summary>
  public async Task RollbackAsync(CancellationToken cancellationToken = default)
  {
    await this.dbLock.WithLockAsync(async () =>
    {
      if (this.transaction?.IsActive == true)
        await this.transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
    }, cancellationToken);
  }

  /// <summary>
  /// Завершить работу: откатить незавершенную транзакцию и освободить сессию.
  /// </summary>
  public async ValueTask DisposeAsync()
  {
    try
    {
      await this.dbLock.WithLockAsync(async () =>
      {
        if (this.transaction?.IsActive == true)
          await this.transaction.RollbackAsync().ConfigureAwait(false);
      });
    }
    finally
    {
      this.transaction?.Dispose();
      this.session?.Dispose();
    }
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="sessionFactory">Фабрика сессий.</param>
  public UnitOfWork(ISessionFactory sessionFactory)
  {
    this.sessionFactory = sessionFactory;
  }

  #region AsyncLock

  /// <summary>
  /// Асинхронный инкапсулированный замок: гарантирует, что одна логическая
  /// операция с БД на сессии выполняется не пересекаясь с другой.
  /// </summary>
  private sealed class AsyncLock
  {
    private readonly SemaphoreSlim semaphore = new(1, 1);

    /// <summary>
    /// Выполнить асинхронную операцию под замком.
    /// </summary>
    public async Task<T> WithLockAsync<T>(Func<Task<T>> action,
      CancellationToken cancellationToken = default)
    {
      await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
      try
      {
        return await action().ConfigureAwait(false);
      }
      finally
      {
        this.semaphore.Release();
      }
    }

    /// <summary>
    /// Выполнить асинхронную операцию, не возвращающую значение, под замком.
    /// </summary>
    public async Task WithLockAsync(Func<Task> action,
      CancellationToken cancellationToken = default)
    {
      await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
      try
      {
        await action().ConfigureAwait(false);
      }
      finally
      {
        this.semaphore.Release();
      }
    }

    /// <summary>
    /// Выполнить синхронную операцию под замком.
    /// </summary>
    public T WithLock<T>(Func<T> action, CancellationToken cancellationToken = default)
    {
      this.semaphore.Wait(cancellationToken);
      try
      {
        return action();
      }
      finally
      {
        this.semaphore.Release();
      }
    }
  }

  #endregion
}
