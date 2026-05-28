using System.Linq.Expressions;
using CommonModels.Attibutes;
using CommonModels.Interfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.Infrastructure;
using NHibernate.Linq;
using Npgsql;

namespace DBCore;

public class DbRepository : IRepository
{
  /// <summary>
  /// Маппинг объектов к сессиям.
  /// </summary>
  private Dictionary<string, ISession> Sessions { get; set; } = new ();

  private readonly ISessionFactory sessionFactory;

  /// <summary>
  /// Добавление в базу данных нового объекта
  /// </summary>
  /// <param name="item">Добавляемый объект</param>
  public void Add(IHasId item)
  {
    ISession session = this.GetSessionForItem(item);
    using ITransaction transaction = session.BeginTransaction();
    session.Save(item);
    transaction.Commit();
  }

  /// <summary>
  /// Обновить свойства объекта в базе данных
  /// </summary>
  /// <param name="item">Объект свойства, которого будут обновляться</param>
  public async Task AddOrUpdate(IHasId item)
  {
    ISession session = this.GetSessionForItem(item);
    using ITransaction transaction = session.BeginTransaction();
    await session.SaveOrUpdateAsync(item);
    await transaction.CommitAsync();
  }

  
  /// <summary>
  /// Обновить свойства объекта в базе данных.
  /// </summary>
  /// <param name="item">Объект свойства, которого будут обновляться</param>
  public async Task Update(IHasId item)
  {
    ISession session = this.GetSessionForItem(item);
    using ITransaction transaction = session.BeginTransaction();
    await session.UpdateAsync(item);
    await transaction.CommitAsync();
  }
  
  /// <summary>
  /// Получить объект по ID
  /// </summary>
  /// <param name="id">ID объекта</param>
  /// <typeparam name="T">Тип объекта</typeparam>
  /// <returns></returns>
  public async Task<T?> GetById<T>(int id)  where T : IHasId
  {
    var session = this.GetSessionForItem(typeof(T), id);
    var result =  await session.GetAsync<T>(id);
    if (result != null)
    {
      this.AddSessionToList(result, session);
    }
    else
    {
      session.Close();
      session.Dispose();
    }
    return result;
  }

  /// <summary>
  /// Удалить сущность из БД.
  /// </summary>
  /// <param name="item">Сущность.</param>
  public async Task Delete(IHasId item)
  {
    ISession session = this.GetSessionForItem(item);
    using ITransaction transaction = session.BeginTransaction();
    await session.DeleteAsync(item);
    await transaction.CommitAsync();
  }

  /// <summary>
  /// Получить объект по свойству и его значению
  /// </summary>
  /// <param name="fieldName">Имя свойства</param>
  /// <param name="value">Значение свойства</param>
  /// <typeparam name="T">Класс объекта</typeparam>
  /// <returns></returns>
  public T GetByField<T>(string fieldName, object value) where T : class, IHasId
  {
    ISession session = this.sessionFactory.OpenSession();
    ICriteria criteria = session.CreateCriteria(typeof(T));
    criteria.Add(Restrictions.Eq(fieldName, value));
    var result =  (T)criteria.UniqueResult();
    this.AddSessionToList(result, session);
    return result;
  }

  /// <summary>
  /// Вернуть список сущностей по условию.
  /// </summary>
  /// <param name="predicate">Условие в виде предиката.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <typeparam name="T">Класс объекта.</typeparam>
  /// <returns>Список сущностей, удовлетворяющих критерию.</returns>
  public async Task<List<T>> GetAsync<T>(Expression<Func<T, bool>> predicate,
    CancellationToken cancellationToken = default) where T : class, IHasId
  {
    var session = this.sessionFactory.OpenSession();
    var results = await session.Query<T>()
        .Where(predicate)
        .ToListAsync(cancellationToken);
    this.AddSessionToList(results, session);
    return results;
  }
  
  /// <summary>
  /// Получить первое значение.
  /// </summary>
  /// <param name="predicate">Условие в виде предиката.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <typeparam name="T">Класс объекта.</typeparam>
  /// <returns>Первая сущность, удовлетворяющее условию.</returns>
  public async Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> predicate,
    CancellationToken cancellationToken = default) where T : class, IHasId
  {
    ISession? session = this.sessionFactory.OpenSession();
    T? result = await session.Query<T>()
      .Where(predicate)
      .FirstAsync(cancellationToken);
    this.AddSessionToList(result, session);
    return result;
  }

  /// <summary>
  /// Получить сессию для объекта.
  /// </summary>
  /// <param name="item">Объект.</param>
  /// <returns>Сессия.</returns>
  private ISession GetSessionForItem(IHasId item)
  {
    string id = this.GetIdentifierForSessionByItem(item);
    if (this.Sessions.TryGetValue(id, out var session))
    {
      if (session.IsOpen)
        return session;
    }
    ISession newSession = this.sessionFactory.OpenSession();
    this.AddSessionToList(item, newSession);
    return newSession;
  }
  
  private ISession GetSessionForItem(Type itemType, int id)
  {
    string identifier = this.GetIdentifierForSessionByItem(itemType, id);
    if (this.Sessions.TryGetValue(identifier, out ISession? session))
    {
      if (session.IsOpen)
        return session;
    }
    ISession newSession = this.sessionFactory.OpenSession();
    this.AddSessionToList(itemType, id, newSession);
    return newSession;
  }

  /// <summary>
  /// Получить строковый идентификатор для сессии по объекту.
  /// </summary>
  /// <param name="item">Объект.</param>
  /// <returns>Идентификатор.</returns>
  private string GetIdentifierForSessionByItem(IHasId item)
  {
    return $"{item.GetType()}_{item.Id}";
  }
  
    /// <summary>
    /// Получить строковый идентификатор для сессии по объекту.
    /// </summary>
    /// <param name="item">Объект.</param>
    /// <returns>Идентификатор.</returns>
    private string GetIdentifierForSessionByItem(Type itemType, int id)
    {
      return $"{itemType.FullName}_{id}";
    }

  /// <summary>
  /// Добавить сессию по объектам к списку.
  /// </summary>
  /// <param name="items">Список объектов.</param>
  /// <param name="session">Сессия.</param>
  private void AddSessionToList(IEnumerable<IHasId> items, ISession session)
  {
    foreach (var item in items)
    {
      var id = this.GetIdentifierForSessionByItem(item);
      if (this.Sessions.TryGetValue(id, out var oldSession))
      {
        oldSession.SaveOrUpdate(item);
        oldSession.Evict(item);
      }
      this.Sessions[this.GetIdentifierForSessionByItem(item)] = session;
    } 
  }
  private void AddSessionToList(Type itemType, int id, ISession session)
  {
    this.Sessions[this.GetIdentifierForSessionByItem(itemType, id)] = session;
  }
  
  private void AddSessionToList(IHasId item, ISession session)
  {
    this.Sessions[this.GetIdentifierForSessionByItem(item)] = session;
  }
  
  /// <summary>
  /// Проверка существования объекта в БД по уникальным полям
  /// </summary>
  /// <param name="item"></param>
  /// <returns>Признак существует ли объект с этими полями в базе данных</returns>
  private bool IsExist(IHasId item)
  {
    var typeOfItem = item.GetType();
    var uniqueProperties = typeOfItem.GetProperties()
      .Where(x => x.GetCustomAttributes(typeof(UniqueAttribute), true).Length != 0)
      .ToList();
    
    foreach (var property in uniqueProperties)
    {
      var existItem = this.GetByField<IHasId>(property.Name, property.GetValue(item));
      if (existItem == null)
        continue;
      throw new ArgumentException(
        $"Элемент типа {typeOfItem} c параметром {property.Name} и значением {property.GetValue(item)} уже существует");
    }
    return false;
  }

  #region IDisposable
  
  public void Dispose()
  {
    foreach (var session in this.Sessions.Values)
    {
      try
      {
        if (session.IsOpen)
        {
          var transaction = session.GetCurrentTransaction();
          if (transaction?.IsActive == true)
            transaction.Rollback();
          session.Close();
        }
      }
      finally
      {
        session.Dispose();
      }
    }
    GC.SuppressFinalize(this);
  }
  #endregion
  
  #region Конструкторы
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  public DbRepository(NhibernateHelper nhibernateHelper)
  {
    this.sessionFactory = nhibernateHelper.SessionFactory;
  }

  /// <summary>
  /// Деструктор.
  /// </summary>
  ~DbRepository()
  {
    this.Dispose();
  }
  #endregion

}