using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CommonModels.Interfaces;

public interface IRepository: IDisposable
{
  public void Add(IHasId item);
  public void AddOrUpdate (IHasId item);
  public List<T> Get<T>(Expression<Func<T, bool>> predicate) where T : IHasId;
  
  /// <summary>
  /// Получить объект по ИД.
  /// </summary>
  /// <param name="id">Ид объекта.</param>
  /// <typeparam name="T">Тип объекта</typeparam>
  /// <returns>Объект из базы данных.</returns>
  public T GetById<T>(int id) where T : IHasId;
}