using CommonModels.Interfaces;
using CommonModels.Models;
using DBCore;

namespace Application;

public abstract class GenericService
{
  /// <summary>
  /// Репозиторий.
  /// </summary>
  protected readonly IRepository repository;
  
  /// <summary>
  /// Конструктор.
  /// </summary>
  public GenericService(IRepository repository)
  {
    this.repository = repository;
  }
}