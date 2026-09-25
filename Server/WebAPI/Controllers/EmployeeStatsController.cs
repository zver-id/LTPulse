using System.Collections.Generic;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;

namespace WebAPI.Controllers;

/// <summary>
/// Контроллер статистики по сотрудникам.
/// </summary>
[ApiController]
[Route("/api/[controller]")]
public class EmployeeStatsController(
  IMapper mapper,
  EmployeeStatsService statsService,
  TeamService teamService)
  : ControllerBase
{
  /// <summary>
  /// Маппер.
  /// </summary>
  private readonly IMapper mapper = mapper;

  /// <summary>
  /// Сервис статистики по сотрудникам.
  /// </summary>
  private readonly EmployeeStatsService statsService = statsService;

  /// <summary>
  /// Сервис команд.
  /// </summary>
  private readonly TeamService teamService = teamService;

  /// <summary>
  /// Получить статистику по сотрудникам команды за период.
  /// </summary>
  /// <param name="teamId">Идентификатор команды.</param>
  /// <param name="dayCount">Длина периода в днях.</param>
  /// <returns>Список статистики по сотрудникам.</returns>
  [HttpGet]
  public async Task<ActionResult<List<EmployeeStatsDTO>>> Get(int teamId, int dayCount)
  {
    var team = await this.teamService.GetTeamById(teamId);
    if (team == null)
      return this.BadRequest("Команда не найдена");

    var stats = await this.statsService.GetStats(teamId, dayCount);
    return this.Ok(this.mapper.Map<List<EmployeeStatsDTO>>(stats));
  }
}
