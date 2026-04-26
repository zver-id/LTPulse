using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CommonModels.Interfaces;

public interface IRepository: IDisposable
{
  public void Add(IHasId item);
  public Task AddOrUpdate (IHasId item);
  public Task<List<T>> GetAsync<T>(Expression<Func<T, bool>> predicate,
    CancellationToken cancellationToken = default) where T : IHasId;

  public Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> predicate,
    CancellationToken cancellationToken = default) where T : IHasId;
  
  /// <summary>
  /// Получить объект по ИД.
  /// </summary>
  /// <param name="id">Ид объекта.</param>
  /// <typeparam name="T">Тип объекта</typeparam>
  /// <returns>Объект из базы данных.</returns>
  public Task<T> GetById<T>(int id) where T : IHasId;

  /// <summary>
  /// Удалить сущность из БД.
  /// </summary>
  /// <param name="item">Сущность.</param>
  public Task Delete(IHasId item);
}