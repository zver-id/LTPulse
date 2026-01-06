using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CommonModels.Interfaces;

public interface IRepository: IDisposable
{
  public void Add(IHasId item);
  public void AddOrUpdate (IHasId item);
  public List<T> Get<T>(Expression<Func<T, bool>> predicate) where T : IHasId;
}