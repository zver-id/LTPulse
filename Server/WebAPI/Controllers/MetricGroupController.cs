using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CommonModels.Interfaces;
using CommonModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.DTO;

namespace WebAPI.Controllers;

/// <summary>
/// Контроллер групп метрик.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetricGroupController(
  IRepository repository,
  ILogger<MetricGroupController> logger,
  IMapper mapper)
  : GenericController(repository, mapper)
{
  /// <summary>
  /// Логгер.
  /// </summary>
  private ILogger<MetricGroupController> Logger {get; set;} = logger;

  /// <summary>
  /// Получить все группы метрик.
  /// </summary>
  /// <returns>Группы метрик.</returns>
  [HttpGet]
  public async Task<ActionResult<List<MetricGroupDTO>>> GetMetricGroup()
  {
    var metricGroups = await this.Repository.GetAsync<MetricGroup>(mg => true);
    return this.Ok(this.Mapper.Map<List<MetricGroupDTO>>(metricGroups));
  }
}