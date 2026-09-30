using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CommonModels.Interfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Infrastructure;
using NHibernate.Linq;

namespace DBCore;

/// <summary>
/// Репозиторий. Тонкая обертка над общей сессией <see cref="IUnitOfWork"/>.
/// </summary>
public class DbRepository : IRepository
{
  private readonly IUnitOfWork unitOfWork;

  /// <summary>
  /// Текущая сессия единицы работы (одна на область жизни).
  /// </summary>
  private ISession Session => this.unitOfWork.Session;

  /// <summary>
  /// Единица работы (владелец сессии) для низкоуровневых операций.
  /// </summary>
  public IUnitOfWork UnitOfWork => this.unitOfWork;

  /// <summary>
  /// Добавить новый объект в базу данных.
  /// </summary>
  /// <param name="item">Добавляемый объект.</param>
  public void Add(IHasId item)
  {
    this.unitOfWork.ExecuteOnSession(session =>
    {
      session.Save(item);
      session.Flush();
      return 0;
    });
  }

  /// <summary>
  /// Добавить объект или обновить существующий.
  /// </summary>
  /// <param name="item">Объект.</param>
  public async Task AddOrUpdate(IHasId item)
  {
    await this.unitOfWork.ExecuteOnSessionAsync(async session =>
    {
      await session.SaveOrUpdateAsync(item);
      await session.FlushAsync();
    });
  }

  /// <summary>
  /// Получить список объектов по условию.
  /// </summary>
  /// <param name="predicate">Условие.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <typeparam name="T">Тип объекта.</typeparam>
  /// <returns>Список объектов, удовлетворяющих условию.</returns>
  public async Task<List<T>> GetAsync<T>(Expression<Func<T, bool>> predicate,
    CancellationToken cancellationToken = default) where T : class, IHasId
  {
    return await this.unitOfWork.ExecuteOnSessionAsync(
      session => session.Query<T>().Where(predicate).ToListAsync(cancellationToken),
      cancellationToken);
  }

  /// <summary>
  /// Получить первый объект по условию.
  /// </summary>
  /// <param name="predicate">Условие.</param>
  /// <param name="cancellationToken">Токен отмены.</param>
  /// <typeparam name="T">Тип объекта.</typeparam>
  /// <returns>Первый объект, удовлетворяющий условию.</returns>
  public async Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> predicate,
    CancellationToken cancellationToken = default) where T : class, IHasId
  {
    return await this.unitOfWork.ExecuteOnSessionAsync(
      session => session.Query<T>().Where(predicate).FirstAsync(cancellationToken),
      cancellationToken);
  }

  /// <summary>
  /// Получить объект по ИД.
  /// </summary>
  /// <param name="id">Ид объекта.</param>
  /// <typeparam name="T">Тип объекта.</typeparam>
  /// <returns>Объект из базы данных. Null, если не найден.</returns>
  public async Task<T> GetById<T>(int id) where T : IHasId
  {
    return await this.unitOfWork.ExecuteOnSessionAsync(session => session.GetAsync<T>(id));
  }

  /// <summary>
  /// Удалить сущность из БД.
  /// </summary>
  /// <param name="item">Сущность.</param>
  public async Task Delete(IHasId item)
  {
    await this.unitOfWork.ExecuteOnSessionAsync(async session =>
    {
      await session.DeleteAsync(item);
      await session.FlushAsync();
    });
  }

  /// <summary>
  /// Обновить сущность в БД.
  /// </summary>
  /// <param name="item">Сущность.</param>
  public async Task Update(IHasId item)
  {
    await this.unitOfWork.ExecuteOnSessionAsync(async session =>
    {
      await session.UpdateAsync(item);
      await session.FlushAsync();
    });
  }

  /// <summary>
  /// Получить объект по свойству и его значению.
  /// </summary>
  /// <param name="fieldName">Имя свойства.</param>
  /// <param name="value">Значение свойства.</param>
  /// <typeparam name="T">Тип объекта.</typeparam>
  /// <returns>Найденный объект. Null, если не найден.</returns>
  public T GetByField<T>(string fieldName, object value) where T : class, IHasId
  {
    return this.unitOfWork.ExecuteOnSession(session =>
    {
      var criteria = session.CreateCriteria(typeof(T));
      criteria.Add(Restrictions.Eq(fieldName, value));
      return (T)criteria.UniqueResult();
    });
  }

  #region IDisposable

  /// <summary>
  /// Освобождает ресурсы сессии единицы работы.
  /// </summary>
  public void Dispose()
  {
    if (this.unitOfWork is IDisposable disposable)
      disposable.Dispose();
    GC.SuppressFinalize(this);
  }

  #endregion

  #region Конструкторы

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="unitOfWork">Единица работы (владелец сессии).</param>
  public DbRepository(IUnitOfWork unitOfWork)
  {
    this.unitOfWork = unitOfWork;
  }

  /// <summary>
  /// Конструктор для сценариев вне DI (например, консольная инициализация БД).
  /// Создает собственную единицу работы на переданной фабрике сессий.
  /// </summary>
  /// <param name="nhibernateHelper">Помощник NHibernate с фабрикой сессий.</param>
  public DbRepository(NhibernateHelper nhibernateHelper)
    : this(new UnitOfWork(nhibernateHelper.SessionFactory))
  {
  }

  #endregion
}
