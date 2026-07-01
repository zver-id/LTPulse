using System;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

/// <summary>
/// Принудительная генерация отчета и информация о нем.
/// </summary>
/// <param name="repository"></param>
/// <param name="mapper"></param>
[ApiController]
[Route("api/[controller]")]
public class GenerateReportController(IRepository repository,
  IMapper mapper,
  GenerateReportService generateReportService) : 
  GenericController(repository, mapper)
{
  /// <summary>
  /// Управление генерацией отчетов.
  /// </summary>
  private GenerateReportService GenerateReportService { get; } = generateReportService;
  
  /// <summary>
  /// Получить последнюю дату генерации отчета.
  /// </summary>
  /// <param name="teamId"></param>
  /// <returns></returns>
  [HttpGet]
  public async Task<ActionResult<DateTime>> GetLastestGenerationTime(int teamId)
  {
    var date = await this.GenerateReportService.GetLastestGenerationTime(teamId);
    if (date == null)
      return this.NoContent();
    return this.Ok(date);
  }
  
}