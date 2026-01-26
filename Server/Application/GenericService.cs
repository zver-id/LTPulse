using CommonModels.Models;
using DBCore;

namespace Application;

public abstract class GenericService
{
  /// <summary>
  /// Репозиторий.
  /// </summary>
  protected readonly DbRepository repository;
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  public GenericService()
  {
    this.repository = new DbRepository();
  }
}