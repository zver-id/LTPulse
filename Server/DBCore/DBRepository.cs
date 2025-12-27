using System.Linq.Expressions;
using CollectionLibrary.Nhibernate.Infrastructure;
using CommonModels.Interfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.Infrastructure;
using NHibernate.Linq;
using Npgsql;

namespace DBCore;

public class DBRepository : IDisposable
{
  /// <summary>
  /// Сессия репозитория.
  /// </summary>
  private ISession Session { get;set; }
  
  /// <summary>
  /// Добавление в базу данных нового объекта
  /// </summary>
  /// <param name="item">Добавляемый объект</param>
  public void Add(IHasId item)
  {
    using (ITransaction  transaction = this.Session.BeginTransaction())
    {
      this.Session.Save(item);
      transaction.Commit();
    }

  }
  /// <summary>
  /// Обновить свойства объекта в базе данных
  /// </summary>
  /// <param name="item">Объект свойства, которого будут обновляться</param>
  public void AddOrUpdate(IHasId item)
  {
    using (ITransaction transaction = this.Session.BeginTransaction())
    {
      this.Session.SaveOrUpdate(item);
      transaction.Commit();
    }
  }
  /// <summary>
  /// Получить объект по ID
  /// </summary>
  /// <param name="id">ID объекта</param>
  /// <typeparam name="T">Тип объекта</typeparam>
  /// <returns></returns>
  public T GetById<T>(int id)
  {
    return this.Session.Get<T>(id);
  }
  /// <summary>
  /// Получить объект по свойству и его значению
  /// </summary>
  /// <param name="fieldName">Имя свойства</param>
  /// <param name="value">Значение свойства</param>
  /// <typeparam name="T">Класс объекта</typeparam>
  /// <returns></returns>
  public T GetByField<T>(string fieldName, object value)
  {
    ICriteria criteria = this.Session.CreateCriteria(typeof(T));
    criteria.Add(Restrictions.Eq(fieldName, value));
    return (T)criteria.UniqueResult();
  }

  /// <summary>
  /// Вернуть список сущностей по условию.
  /// </summary>
  /// <param name="predicate">Условие в виде предиката.</param>
  /// <typeparam name="T">Класс объекта.</typeparam>
  /// <returns>Список сущностей, удовлетворяющих критерию.</returns>
  public List<T> Get<T>(Expression<Func<T, bool>> predicate) where T : class
  {
    return this.Session.Query<T>()
        .Where(predicate)
        .ToList();
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
    if (this.Session != null)
    { 
      try
      {
        if (this.Session.IsOpen)
        {
          var transaction = this.Session.GetCurrentTransaction();
          if (transaction?.IsActive == true)
            transaction.Rollback();
          Session.Close();
        }
      }
      finally
      {
        this.Session.Dispose(); 
        this.Session = null;
      }
    }
    GC.SuppressFinalize(this);
  }
  #endregion
  
  #region Конструкторы
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  public DBRepository()
  {
    this.Session = NhibernateHelper.OpenSession();
  }

  /// <summary>
  /// Деструктор.
  /// </summary>
  ~DBRepository()
  {
    this.Dispose();
  }
  #endregion

}