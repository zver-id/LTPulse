using AutoMapper;
using CommonModels.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

public class GenericController(IRepository repository, IMapper mapper) : ControllerBase
{
  /// <summary>
  /// Репозиторий.
  /// </summary>
  protected IRepository Repository {get; set;} = repository;
  
  /// <summary>
  /// Маппер.
  /// </summary>
  protected IMapper Mapper {get; set;} = mapper;
}